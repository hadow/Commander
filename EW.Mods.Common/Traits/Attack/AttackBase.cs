using System;
using System.Linq;
using System.Collections.Generic;
using EW.Traits;
using EW.Activities;
using EW.Mods.Common.Warheads;
using EW.Framework;
using System.Drawing;
namespace EW.Mods.Common.Traits
{
    public abstract class AttackBaseInfo:ConditionalTraitInfo
    {
        public readonly string[] Armaments = { "primary", "secondary" };

        public readonly string Cursor = null;

        public readonly string OutsideRangeCursor = null;
        /// <summary>
        /// Does the attack type require the attacker to enter the target's cell?
        /// </summary>
        public readonly bool AttackRequiresEnteringCell = false;

        /// <summary>
        /// Does not care about shroud or fog.
        /// Enables the actor to launch an attack against a target even if he has no visibility of it.
        /// </summary>
        public readonly bool IgnoresVisibility = false;

        [VoiceReference]
        public readonly string Voice = "Action";

        public abstract override object Create(ActorInitializer init);

    }


    /// <summary>
    /// 攻击
    /// </summary>
    public abstract class AttackBase:ConditionalTrait<AttackBaseInfo>,IIssueOrder,IResolveOrder,IOrderVoice
    {
        readonly string attackOrderName = "Attack";
        readonly string forceAttackOrderName = "ForceAttack";

        [Sync]
        public bool IsAniming { get; internal set; }

        protected IFacing facing;

        protected Building building;

        protected IPositionable positionable;

        protected Func<IEnumerable<Armament>> getArmaments;//获取武器装备

        public IEnumerable<Armament> Armaments { get { return getArmaments(); } }
        readonly Actor self;

        public AttackBase(Actor self,AttackBaseInfo info) : base(info)
        {
            this.self = self;
            
        }

        protected override void Created(Actor self)
        {
            facing = self.TraitOrDefault<IFacing>();
            building = self.TraitOrDefault<Building>();
            positionable = self.TraitOrDefault<IPositionable>();

            getArmaments = InitializeGetArmaments(self);

            base.Created(self);
        }

        protected virtual Func<IEnumerable<Armament>> InitializeGetArmaments(Actor self)
        {
            var armaments = self.TraitsImplementing<Armament>().Where(a => Info.Armaments.Contains(a.Info.Name)).ToArray();
            return () => armaments;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        protected virtual bool CanAttack(Actor self,Target target)
        {
            if (!self.IsInWorld || IsTraitDisabled || self.IsDisabled())
                return false;

            if (!target.IsValidFor(self))
                return false;

            if (!HasAnyValidWeapons(target))
                return false;

            if (building != null && !building.BuildComplete)
                return false;

            if (Armaments.All(a => a.IsReloading))
                return false;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="allowMove"></param>
        /// <returns></returns>
        public bool IsReachableTarget(Target target,bool allowMove)
        {
            return HasAnyValidWeapons(target) && (target.IsInRange(self.CenterPosition, GetMaximumRangeVersusTarget(target)) || (allowMove && self.Info.HasTraitInfo<IMoveInfo>()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public WDist GetMaximumRangeVersusTarget(Target target)
        {
            if (IsTraitDisabled)
                return WDist.Zero;

            //PERF:Avoid LINQ
            var max = WDist.Zero;
            foreach(var armament in Armaments)
            {
                if (armament.IsTraitDisabled)
                    continue;

                if (armament.OutOfAmo)
                    continue;

                if (!armament.Weapon.IsValidAgainst(target, self.World, self))
                    continue;

                var range = armament.MaxRange();
                if (max < range)
                    max = range;
            }

            return max;
        }

        public IEnumerable<IOrderTargeter> Orders
        {
            get
            {
                if (IsTraitDisabled)
                    yield break;

                var armament = Armaments.FirstOrDefault(a => a.Weapon.Warheads.Any(w => (w is DamageWarhead)));
                if (armament == null)
                    yield break;

                var negativeDamage = (armament.Weapon.Warheads.FirstOrDefault(w => (w is DamageWarhead)) as DamageWarhead).Damage < 0;

                yield return new AttackOrderTargeter(this, 6, negativeDamage);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="order"></param>
        /// <param name="target"></param>
        /// <param name="queued"></param>
        /// <returns></returns>
        public Order IssueOrder(Actor self,IOrderTargeter order,Target target,bool queued)
        {
            if(order is AttackOrderTargeter)
            {
                switch (target.Type)
                {
                    case TargetT.Actor:
                        return new Order(order.OrderID, self, queued) { TargetActor = target.Actor };
                    case TargetT.FrozenActor:
                        return new Order(order.OrderID, self, queued) { ExtraData = target.FrozenActor.ID };
                    case TargetT.Terrain:
                        return new Order(order.OrderID, self, queued) { TargetLocation = self.World.Map.CellContaining(target.CenterPosition) };
                }
            }
            return null;
        }

        public virtual void ResolveOrder(Actor self,Order order)
        {
            var forceAttack = order.OrderString == forceAttackOrderName;

            if(forceAttack || order.OrderString == attackOrderName)
            {
                var target = self.ResolveFrozenActorOrder(order, Color.Red);
                if (!target.IsValidFor(self))
                    return;

                self.SetTargetLine(target, Color.Red);

            }
        }

        public void AttackTarget(Target target,bool queued,bool allowMove,bool forceAttack = false)
        {
            if (self.IsDisabled() || IsTraitDisabled)
                return;

            if (!target.IsValidFor(self))
                return;

            if (!queued)
                self.CancelActivity();

            self.QueueActivity(GetAttackActivity(self, target, allowMove, forceAttack));
        }

        public abstract Activity GetAttackActivity(Actor self, Target newTarget, bool allowMove, bool forceAttack);

        public string VoicePhraseForOrder(Actor self,Order order)
        {
            return order.OrderString == attackOrderName || order.OrderString == forceAttackOrderName ? Info.Voice : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="checkForCenterTargetingWeapons"></param>
        /// <returns></returns>
        public bool HasAnyValidWeapons(Target t,bool checkForCenterTargetingWeapons = false)
        {
            if (IsTraitDisabled)
                return false;
            if (Info.AttackRequiresEnteringCell && (positionable == null || !positionable.CanEnterCell(t.Actor.Location, null, false)))
                return false;

            //PERF: Avoid LINQ
            foreach(var armament in Armaments)
            {
                var checkIsValid = checkForCenterTargetingWeapons ? armament.Weapon.TargetActorCenter : !armament.OutOfAmo;
                if (checkIsValid && !armament.IsTraitDisabled && armament.Weapon.IsValidAgainst(t, self.World, self))
                    return true;
            }
            return false;
        }

        public Stance UnforcedAttackTargetStances()
        {
            //PERF: Avoid LINQ
            var stance = Stance.None;
            foreach(var armament in Armaments)
            {
                if (!armament.IsTraitDisabled)
                    stance |= armament.Info.TargetStances;
            }
            return stance;
        }

        public WDist GetMaximumRange()
        {
            if (!IsTraitDisabled)
                return WDist.Zero;

            //PERF: Avoid LINQ;
            var max = WDist.Zero;
            foreach(var armament in Armaments)
            {
                if (armament.IsTraitDisabled)
                    continue;

                if (armament.OutOfAmo)
                    continue;

                var range = armament.MaxRange();
                if (max < range)
                    max = range;
            }

            return max;
        }

        public WDist GetMinimumRange()
        {
            if (IsTraitDisabled)
                return WDist.Zero;

            //PERF:Avoid LINQ;
            var min = WDist.MaxValue;
            foreach(var armament in Armaments)
            {
                if (armament.IsTraitDisabled)
                    continue;
                if (armament.OutOfAmo)
                    continue;

                var range = armament.Weapon.MinRange;
                if (min > range)
                    min = range;

            }

            return min != WDist.MaxValue ? min : WDist.Zero;
        }

        /// <summary>
        /// 为目标挑选武器
        /// </summary>
        /// <param name="t"></param>
        /// <param name="forceAttack"></param>
        /// <returns></returns>
        public IEnumerable<Armament> ChooseArmamentsForTarget(Target t,bool forceAttack)
        {
            if ((!forceAttack) && (t.Type == TargetT.Terrain || t.Type == TargetT.Invalid))
                return Enumerable.Empty<Armament>();

            Player owner = null;

            if(t.Type == TargetT.FrozenActor)
            {
                owner = t.FrozenActor.Owner;
            }
            else if(t.Type == TargetT.Actor)
            {

            }

            return Armaments.Where(a => 
                !a.IsTraitDisabled 
                &&(owner == null || (forceAttack ? a.Info.ForceTargetsStance:a.Info.TargetStances).HasStance(self.Owner.Stances[owner]))
                && a.Weapon.IsValidAgainst(t,self.World,self));
        }

        /// <summary>
        /// 攻击指令锁定目标
        /// </summary>
        class AttackOrderTargeter : IOrderTargeter
        {
            readonly AttackBase ab;

            public string OrderID { get; private set; }

            public int OrderPriority { get; private set; }

            public AttackOrderTargeter(AttackBase ab,int priority,bool negativeDamage)
            {
                this.ab = ab;
                OrderID = ab.attackOrderName;
                OrderPriority = priority;
            }
            bool CanTargetActor(Actor self,Target target,ref TargetModifiers modifiers,ref string cursor)
            {
                return true;
            }

            bool CanTargetLocation(Actor self,CPos location,List<Actor> actorsAtLocation,TargetModifiers modifiers,ref string cursor)
            {
                return true;
            }
            public bool CanTarget(Actor self,Target target,List<Actor> othersAtTarget,ref TargetModifiers modifiers,ref string cursor)
            {
                switch (target.Type)
                {
                    case TargetT.Actor:
                    case TargetT.FrozenActor:
                        return CanTargetActor(self, target, ref modifiers, ref cursor);
                    case TargetT.Terrain:
                        return CanTargetLocation(self, self.World.Map.CellContaining(target.CenterPosition), othersAtTarget,modifiers,ref cursor);
                    default:
                        return false;
                            
                }
            
            }

            public bool TargetOverridesSelection(TargetModifiers modifiers) { return true; }

            public bool IsQueued { get; protected set; }
        }
    }
}
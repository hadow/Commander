using System;
using System.Linq;
using System.Collections.Generic;
using EW.Traits;
using EW.Activities;
using EW.Mods.Common.Warheads;
using System.Drawing;
using EW.NetWork;
namespace EW.Mods.Common.Traits
{
    public abstract class AttackBaseInfo:PausableConditionalTraitInfo
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
    public abstract class AttackBase:PausableConditionalTrait<AttackBaseInfo>,IIssueOrder,IResolveOrder,IOrderVoice
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

        /// <summary>
        /// 初始化武器
        /// </summary>
        /// <returns>The get armaments.</returns>
        /// <param name="self">Self.</param>
        protected virtual Func<IEnumerable<Armament>> InitializeGetArmaments(Actor self)
        {
            var armaments = self.TraitsImplementing<Armament>().Where(a => Info.Armaments.Contains(a.Info.Name)).ToArray();
            return () => armaments;
        }

        /// <summary>
        /// 检查当前可否攻击目标
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        protected virtual bool CanAttack(Actor self,Target target)
        {
            if (!self.IsInWorld || IsTraitDisabled || IsTraitPaused)
                return false;

            if (!target.IsValidFor(self))
                return false;

            if (!HasAnyValidWeapons(target))
                return false;

            var mobile = self.TraitOrDefault<Mobile>();
            if (mobile != null && !mobile.CanInteractWithGroundLayer(self))
                return false;
            
                
            //Building is under construction or is being sold.
            if (building != null && !building.BuildComplete)
                return false;

            if (Armaments.All(a => a.IsReloading))//武器正在重载
                return false;

            return true;
        }

        public virtual void DoAttack(Actor self,Target target,IEnumerable<Armament> armaments = null)
        {
            if (armaments == null && !CanAttack(self, target))
                return;
            foreach (var a in armaments ?? Armaments)
                a.CheckFire(self, facing, target);
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

                if (armament.IsTraitPaused)
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
        Order IIssueOrder.IssueOrder(Actor self,IOrderTargeter order,Target target,bool queued)
        {
            if(order is AttackOrderTargeter)
            {
                return new Order(order.OrderID, self, target, queued);
                //switch (target.Type)
                //{
                //    case TargetT.Actor:
                //        return new Order(order.OrderID, self, queued) { TargetActor = target.Actor };
                //    case TargetT.FrozenActor:
                //        return new Order(order.OrderID, self, queued) { ExtraData = target.FrozenActor.ID };
                //    case TargetT.Terrain:
                //        return new Order(order.OrderID, self, queued) { TargetLocation = self.World.Map.CellContaining(target.CenterPosition) };
                //}
            }
            return null;
        }

        /// <summary>
        /// EWs the . traits. IR esolve order. resolve order.
        /// </summary>
        /// <param name="self">Self.</param>
        /// <param name="order">Order.</param>
        void IResolveOrder.ResolveOrder(Actor self,Order order)
        {
            var forceAttack = order.OrderString == forceAttackOrderName;

            if(forceAttack || order.OrderString == attackOrderName)
            {
                var target = self.ResolveFrozenActorOrder(order, Color.Red);
                if (!target.IsValidFor(self))
                    return;

                self.SetTargetLine(target, Color.Red);
                AttackTarget(target,order.Queued,true,forceAttack);
            }

            if (order.OrderString == "Stop")
                OnStopOrder(self);
        }


        protected virtual void OnStopOrder(Actor self){
            self.CancelActivity();
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


        public virtual WPos GetTargetPosition(WPos pos,Target target)
        {
            return HasAnyValidWeapons(target, true) ? target.CenterPosition : target.Positions.PositionClosestTo(pos);
        }

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
                var checkIsValid = checkForCenterTargetingWeapons ? armament.Weapon.TargetActorCenter : !armament.IsTraitPaused;
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

        /// <summary>
        /// 获取武器最大攻击范围
        /// </summary>
        /// <returns></returns>
        public WDist GetMaximumRange()
        {
            if (IsTraitDisabled)
                return WDist.Zero;

            //PERF: Avoid LINQ;
            var max = WDist.Zero;
            foreach(var armament in Armaments)
            {
                if (armament.IsTraitDisabled)
                    continue;

                if (armament.IsTraitPaused)
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
                if (armament.IsTraitPaused)
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
                owner = t.Actor.EffectiveOwner != null && t.Actor.EffectiveOwner.Owner != null ? t.Actor.EffectiveOwner.Owner : t.Actor.Owner;

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
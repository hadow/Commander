using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using System.Drawing;
using EW.NetWork;
namespace EW.Mods.Common.Traits
{

    public class AutoTargetInfo : ConditionalTraitInfo, IRulesetLoaded, Requires<AttackBaseInfo>,UsesInit<StanceInit>
    {
        /// <summary>
        /// It will try to hunt down the enemy if it is not set to defend.
        /// </summary>
        public readonly bool AllowMovement = true;

        public readonly int ScanRadius = -1;        //set to a value>1 to override weapons maximum range for this.

        public readonly UnitStance InitialStanceAI = UnitStance.AttackAnything;

        public readonly UnitStance InitialStance = UnitStance.Defend;

        [GrantedConditionReference]
        public readonly string HoldFireCondition = null;

        [GrantedConditionReference]
        public readonly string ReturnFireCondition = null;

        [GrantedConditionReference]
        public readonly string DefendCondition = null;

        [GrantedConditionReference]
        public readonly string AttackAnythingCondition = null;

        public readonly Dictionary<UnitStance, string> ConditionByStance = new Dictionary<UnitStance, string>();

        //Allow the player to change the unit stance.
        public readonly bool EnableStance = true;

        public readonly int MinimumScanTimeInterval = 3;

        public readonly int MaximumScanTimeInterval = 8;

        //public readonly bool TargetWhenIdle = true;

        //public readonly bool TargetWhenDamaged = true;


        public override object Create(ActorInitializer init)
        {
            return new AutoTarget(init, this);
        }

        public override void RulesetLoaded(Ruleset rules, ActorInfo info)
        {
            base.RulesetLoaded(rules, info);

            if (HoldFireCondition != null)
                ConditionByStance[UnitStance.HoldFire] = HoldFireCondition;

            if (ReturnFireCondition != null)
                ConditionByStance[UnitStance.ReturnFire] = ReturnFireCondition;

            if (DefendCondition != null)
                ConditionByStance[UnitStance.Defend] = DefendCondition;

            if (AttackAnythingCondition != null)
                ConditionByStance[UnitStance.AttackAnything] = AttackAnythingCondition;
        }
    }


    public class AutoTarget:ConditionalTrait<AutoTargetInfo>,ITick,ISync,INotifyIdle,INotifyDamage,IResolveOrder,INotifyCreated
    {
        //readonly AttackBase[] attackBases;
        readonly IEnumerable<AttackBase> activeAttackBases;
        readonly AttackFollow[] attackFollows;

        [Sync]
        int nextScanTime = 0;

        [Sync]
        public Actor Aggressor; //侵略者

        [Sync]
        public Actor TargetedActor;

        public UnitStance Stance
        {
            get { return stance; }
        }

        UnitStance stance;

        ConditionManager conditionManager;

        AutoTargetPriority[] targetPriorities;

        int conditionToken = ConditionManager.InvalidConditionToken;


        public AutoTarget(ActorInitializer init,AutoTargetInfo info) : base(info)
        {
            var self = init.Self;
            activeAttackBases = self.TraitsImplementing<AttackBase>().ToArray().Where(Exts.IsTraitEnabled);
        }

        void INotifyIdle.TickIdle(Actor self)
        {
            if (IsTraitDisabled || Stance < UnitStance.Defend)
                return;

            bool allowMove;
            if(ShouldAttack(out allowMove))
            {
                ScanAndAttack(self, allowMove);
            }
            
        }

        bool ShouldAttack(out bool allowMove)
        {
            allowMove = Info.AllowMovement && Stance != UnitStance.Defend;

            //PERF: Avoid LINQ;
            foreach (var attackFollow in attackFollows)
                if (!attackFollow.IsTraitDisabled && attackFollow.IsReachableTarget(attackFollow.Target, allowMove))
                    return false;

            return true;
        }

        void Attack(Actor self,Actor targetActor,bool allowMove)
        {
            TargetedActor = targetActor;
            var target = Target.FromActor(targetActor);
            self.SetTargetLine(target, Color.Red, false);

            foreach (var ab in activeAttackBases)
                ab.AttackTarget(target, false, allowMove);
        }

        void ITick.Tick(Actor self)
        {
            if (IsTraitDisabled)
                return;

            if (nextScanTime > 0)
                --nextScanTime;
        }

        void INotifyCreated.Created(Actor self)
        {
            conditionManager = self.TraitOrDefault<ConditionManager>();
            targetPriorities = self.TraitsImplementing<AutoTargetPriority>().ToArray();
            ApplyStanceCondition(self);
        }

        void IResolveOrder.ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString == "SetUnitStance" && Info.EnableStance)
                SetStance(self, (UnitStance)order.ExtraData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        public void SetStance(Actor self,UnitStance value)
        {
            if (stance == value)
                return;

            stance = value;
            ApplyStanceCondition(self);
        }

        void ApplyStanceCondition(Actor self)
        {
            if (conditionManager == null)
                return;
            if (conditionToken != ConditionManager.InvalidConditionToken)
                conditionToken = conditionManager.RevokeCondition(self, conditionToken);

            string condition;
            if (Info.ConditionByStance.TryGetValue(stance, out condition))
                conditionToken = conditionManager.GrantCondition(self, condition);
        }

        public void ScanAndAttack(Actor self,bool allowMove)
        {
            var targetActor = ScanForTarget(self, allowMove);
            if (targetActor != null)
                Attack(self, targetActor, allowMove);
        }

        public Actor ScanForTarget(Actor self,bool allowMove)
        {
            if(nextScanTime <=0 && activeAttackBases.Any())
            {
                nextScanTime = self.World.SharedRandom.Next(Info.MinimumScanTimeInterval, Info.MaximumScanTimeInterval);

                foreach(var ab in activeAttackBases)
                {
                    var attackStances = ab.UnforcedAttackTargetStances();
                    if(attackStances != EW.Traits.Stance.None)
                    {
                        var range = Info.ScanRadius > 0 ? WDist.FromCells(Info.ScanRadius) : ab.GetMaximumRange();
                        return ChooseTarget(self, ab, attackStances, range, allowMove);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 选择攻击目标
        /// </summary>
        /// <param name="self"></param>
        /// <param name="ab"></param>
        /// <param name="attackStances"></param>
        /// <param name="scanRange"></param>
        /// <param name="allowMove"></param>
        /// <returns></returns>
        Actor ChooseTarget(Actor self,AttackBase ab,Stance attackStances,WDist scanRange,bool allowMove)
        {
            Actor chosenTarget = null;
            var chosenTargetPriority = int.MinValue;
            int chosenTargetRange = 0;

            var activePriorities = targetPriorities.Where(Exts.IsTraitEnabled)
                .Select(at => at.Info)
                .OrderByDescending(ati => ati.Priority)
                .ToList();

            if (!activePriorities.Any())
                return null;

            var actorsInRange = self.World.FindActorsInCircle(self.CenterPosition, scanRange);
            foreach(var actor in actorsInRange)
            {
                //PERF:Most units can only attack enemy units.If this is the case but the target is not an enemy,
                //We can bail early and avoid the more expensive targeting checks and armaments selection.
                //For groups of allied units.this helps significantly reduce the cost of auto target scans.
                //This is important as these groups will continuously rescan their allies until an enemy finally comes into range.
                //
                if (attackStances == EW.Traits.Stance.Enemy && !actor.AppearsHostileTo(self))
                    continue;

                var targetTypes = actor.TraitsImplementing<ITargetable>().Where(Exts.IsTraitEnabled).SelectMany(t => t.TargetTypes).ToHashSet();

                var target = Target.FromActor(actor);
                var validPriorities = activePriorities.Where(ati =>
                {
                    //Already have a higher priority target.
                    if (ati.Priority < chosenTargetPriority)
                        return false;

                    //Incompatible target types.
                    if (!targetTypes.Overlaps(ati.ValidTargets) || targetTypes.Overlaps(ati.InvalidTargets))
                        return false;
                    return true;

                }).ToList();

                if (!validPriorities.Any() || PreventsAutoTarget(self,actor) || !actor.CanBeViewedByPlayer(self.Owner))
                    continue;

                //Make sure that we can actually fire on the actor.
                var armaments = ab.ChooseArmamentsForTarget(target,false);

                if (!allowMove)
                    armaments = armaments.Where(arm => target.IsInRange(self.CenterPosition, arm.MaxRange()) && !target.IsInRange(self.CenterPosition, arm.Weapon.MinRange));

                if (!armaments.Any())
                    continue;

                //Evaluate whether we want to target this actor.
                var targetRange = (target.CenterPosition - self.CenterPosition).Length;
                foreach(var ati in validPriorities)
                {
                    if(chosenTarget == null || chosenTargetPriority < ati.Priority || (chosenTargetPriority == ati.Priority && targetRange < chosenTargetRange))
                    {
                        chosenTarget = actor;
                        chosenTargetPriority = ati.Priority;
                        chosenTargetRange = targetRange;
                    }
                }
            }

            return chosenTarget;

        }

        bool PreventsAutoTarget(Actor attacker,Actor target)
        {
            foreach (var pat in target.TraitsImplementing<IPreventsAutoTarget>())
                if (pat.PreventsAutoTarget(target, attacker))
                    return true;

            return false;
        }


        void INotifyDamage.Damaged(Actor self, AttackInfo attackInfo)
        {
            if (IsTraitDisabled || !self.IsIdle || Stance < UnitStance.ReturnFire)
                return;

            if (attackInfo.Damage.Value < 0)
                return;

            var attacker = attackInfo.Attacker;
            if (attacker.Disposed)
                return;

            if (!attacker.IsInWorld)
            {
                //If the aggressor is in a transport,then attack the transport instead.
                //如果攻击者在交通工具上，那就攻击交通工具
                var passenger = attacker.TraitOrDefault<Passenger>();
                if (passenger != null && passenger.Transport != null)
                    attacker = passenger.Transport;
            }

            var attackerAsTarget = Target.FromActor(attacker);

            if (!activeAttackBases.Any(a => a.HasAnyValidWeapons(attackerAsTarget)))
                return;

            //Don't retaliate against own units force-firing on us.It's usually not what the player wanted.
            //不要对自己的部队进行报复，这通常不是玩家想要的结果
            if (attacker.AppearsFriendlyTo(self))
                return;

            Aggressor = attacker;

            bool allowMove;
            if (ShouldAttack(out allowMove))
                Attack(self, Aggressor, allowMove);
        }
    }


    //class AutoTargetIgnoreInfo : TraitInfo<AutoTargetIgnore>
    //{

    //}

    //class AutoTargetIgnore
    //{

    //}

    public enum UnitStance { HoldFire,ReturnFire,Defend,AttackAnything}

    public class StanceInit : IActorInit<UnitStance>
    {
        [FieldFromYamlKey]
        readonly UnitStance value = UnitStance.AttackAnything;

        public StanceInit() { }

        public StanceInit(UnitStance init)
        {
            value = init;
        }

        public UnitStance Value(World world) { return value; }
    }
}
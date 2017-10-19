using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class AutoTargetInfo : ConditionalTraitInfo,Requires<AttackBaseInfo>,UsesInit<StanceInit>
    {
        public readonly bool AllowMovement = true;

        public readonly int ScanRadius = -1;        //set to a value>1 to override weapons maimum range for this.

        public readonly UnitStance InitialStanceAI = UnitStance.AttackAnything;

        public readonly UnitStance InitialStance = UnitStance.Defend;

        public readonly bool EnableStance = true;

        public readonly int MinimumScanTimeInterval = 3;

        public readonly int MaximumScanTimeInterval = 8;

        public readonly bool TargetWhenIdle = true;

        public readonly bool TargetWhenDamaged = true;


        public override object Create(ActorInitializer init)
        {
            return new AutoTarget(init, this);
        }
    }


    public class AutoTarget:ConditionalTrait<AutoTargetInfo>,ITick,ISync
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

        public UnitStance Stance;

        ConditionManager conditionManager;

        AutoTargetPriority[] targetPriorities;

        int conditionToken = ConditionManager.InvalidConditionToken;


        public AutoTarget(ActorInitializer init,AutoTargetInfo info) : base(info)
        {
            var self = init.Self;
            activeAttackBases = self.TraitsImplementing<AttackBase>().ToArray().Where(Exts.IsTraitEnabled);
        }

        public void Tick(Actor self)
        {

        }

        public void ScanAndAttack(Actor self,bool allowMove)
        {

        }

        public Actor ScanForTarget(Actor self,bool allowMove)
        {
            if(nextScanTime <=0 && activeAttackBases.Any())
            {
                nextScanTime = self.World.SharedRandom.Next(Info.MinimumScanTimeInterval, Info.MaximumScanTimeInterval);

                foreach(var ab in activeAttackBases)
                {
                    var attackStance = ab.UnforcedAttackTargetStances();
                    if(attackStance != EW.Traits.Stance.None)
                    {
                        var range = Info.ScanRadius > 0 ? WDist.FromCells(Info.ScanRadius) : ab.GetMaximumRange();
                    }
                }
            }
        }

        /// <summary>
        /// 
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

            var activePriorities = targetPriorities.Where(Exts.IsTraitEnabled).Select(at => at.Info).OrderByDescending(ati => ati.Priority).ToList();

            if (!activePriorities.Any())
                return null;

            var actorsInRange = self.World.FindActorsInCircle(self.CenterPosition, scanRange);
            foreach(var actor in actorsInRange)
            {
                if (attackStances == EW.Traits.Stance.Enemy && !actor.AppearsHostileTo(self))
                    continue;

                var targetTypes = actor.TraitsImplementing<ITargetable>().Where(Exts.IsTraitEnabled).SelectMany(t => t.TargetTypes).ToHashSet();

                var target = Target.FromActor(actor);
                var validPriorities = activePriorities.Where(ati =>
                {
                    if (ati.Priority < chosenTargetPriority)
                        return false;

                    //Incompatible target types.
                    if (!targetTypes.Overlaps(ati.ValidTargets) || targetTypes.Overlaps(ati.InvalidTargets))
                        return false;
                    return true;

                }).ToList();

                if (!validPriorities.Any() ||!self.Owner.CanTargetActor(actor))
                    continue;
                //
                var armaments = ab.ChooseArmamentsForTarget(target,false);

                if(!allowMove)
                    armaments = armaments.Where(arm=> target.)
            }

            return chosenTarget;

        }
    }


    class AutoTargetIgnoreInfo : TraitInfo<AutoTargetIgnore>
    {

    }

    class AutoTargetIgnore
    {

    }

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
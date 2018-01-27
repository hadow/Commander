using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Warheads
{


    public enum ImpactType
    {
        None,
        Ground,
        Air,
        TargetHit
    }

    public enum ImpactTargetType
    {
        NoActor,
        ValidActor,
        InvalidActor
    }

    public abstract class Warhead:IWarHead
    {

        /// <summary>
        /// What types of targets are affected.
        /// </summary>
        public readonly HashSet<string> ValidTargets = new HashSet<string> { "Ground","Water"};

        public readonly HashSet<string> InvalidTargets = new HashSet<string>();

        /// <summary>
        /// What diplomatic(外交上的) stances are affected.
        /// </summary>
        public readonly Stance ValidStances = Stance.Ally | Stance.Neutral | Stance.Enemy;

        /// <summary>
        /// Can this warhead affect the actor that fired it.
        /// </summary>
        public readonly bool AffectsParent = false;

        /// <summary>
        /// Delay in ticks before applying the warhead effect.(0 - instant)
        /// </summary>
        public readonly int Delay = 0;

        int IWarHead.Delay { get { return Delay; } }

        public bool IsValidTarget(IEnumerable<string> targetTypes)
        {
            return ValidTargets.Overlaps(targetTypes) && !InvalidTargets.Overlaps(targetTypes);
        }



        public abstract void DoImpact(Target target, Actor firedBy, IEnumerable<int> damageModifiers);

        /// <summary>
        /// Checks if the wardhead is valid against (can target ) the actor.
        /// </summary>
        /// <param name="victim"></param>
        /// <param name="firedBy"></param>
        /// <returns></returns>
        public virtual bool IsValidAgainst(Actor victim,Actor firedBy)
        {
            if (!AffectsParent && victim == firedBy)
                return false;

            var stance = firedBy.Owner.Stances[victim.Owner];

            if (!ValidStances.HasStance(stance))
                return false;

            //A target type is valid if it is in the valid targets list,and not in the invalid targets list.
            if (!IsValidTarget(victim.GetEnabledTargetTypes()))
                return false;

            return true;
        }

        public bool IsValidAgainst(FrozenActor victim,Actor firedBy)
        {

            var stance = firedBy.Owner.Stances[victim.Owner];
            if (!ValidStances.HasStance(stance))
                return false;

            if (!IsValidTarget(victim.TargetTypes))
                return false;

            return true;
        }
  
    }
}
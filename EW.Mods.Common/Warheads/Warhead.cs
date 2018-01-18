using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Warheads
{
    public abstract class Warhead:IWarHead
    {

        /// <summary>
        /// What types of targets are affected.
        /// </summary>
        public readonly HashSet<string> ValidTargets = new HashSet<string>();

        public readonly HashSet<string> InvalidTargets = new HashSet<string>();

        public readonly Stance ValidStances = Stance.Ally | Stance.Neutral | Stance.Enemy;

        public readonly bool AffectsParent = false;


        public readonly int Delay = 0;

        int IWarHead.Delay { get { return Delay; } }

        public bool IsValidTarget(IEnumerable<string> targetTypes)
        {
            return ValidTargets.Overlaps(targetTypes) && !InvalidTargets.Overlaps(targetTypes);
        }



        public abstract void DoImpact(Target target, Actor firedBy, IEnumerable<int> damagedModifiers);

        public virtual bool IsValidAgainst(Actor victim,Actor firedBy)
        {
            return false;
        }

        public bool IsValidAgainst(FrozenActor victim,Actor firedBy) { return false; }
  
    }
}
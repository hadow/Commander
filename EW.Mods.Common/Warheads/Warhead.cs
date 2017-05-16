using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Warheads
{
    public abstract class Warhead:IWarHead
    {
        public readonly int Delay = 0;

        int IWarHead.Delay { get { return Delay; } }

        public bool IsValidTarget(IEnumerable<string> targetTypes)
        {
            return false;
        }

        public abstract void DoImpact(Target target, Actor firedBy, IEnumerable<int> damagedModifiers);

        public virtual bool IsValidAgainst(Actor victim,Actor firedBy)
        {
            return false;
        }

        public bool IsValidAgainst(FrozenActor victim,Actor firedBy) { return false; }
  
    }
}
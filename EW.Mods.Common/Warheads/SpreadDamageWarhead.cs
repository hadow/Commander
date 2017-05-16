using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Warheads
{
    public class SpreadDamageWarhead:DamageWarhead
    {
        public override void DoImpact(Target target, Actor firedBy, IEnumerable<int> damagedModifiers)
        {
            throw new NotImplementedException();
        }
    }
}
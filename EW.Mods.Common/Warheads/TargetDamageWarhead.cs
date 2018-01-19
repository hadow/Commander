using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Warheads
{
    public class TargetDamageWarhead:DamageWarhead
    {
        public override void DoImpact(Target target, Actor firedBy, IEnumerable<int> damageModifiers)
        {
        }


        public override void DoImpact(Actor victim, Actor firedBy, IEnumerable<int> damageModifiers)
        {
            base.DoImpact(victim, firedBy, damageModifiers);
        }


        public override void DoImpact(WPos pos, Actor firedBy, IEnumerable<int> damageModifiers)
        {

        }

    }
}
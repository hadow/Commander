using System;

namespace EW.Mods.Common.Traits
{

    public class AttackHeliInfo : AttackFrontalInfo
    {

    }
    public class AttackHeli:AttackFrontal
    {

        public AttackHeli(Actor self,AttackHeliInfo info) : base(self, info) { }
    }
}
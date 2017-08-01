using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class AttackChargeInfo : AttackOmniInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new AttackCharge(init.Self, this);
        }
    }
    public class AttackCharge:AttackOmni
    {
        public AttackCharge(Actor self,AttackChargeInfo info) : base(self, info) { }
    }
}
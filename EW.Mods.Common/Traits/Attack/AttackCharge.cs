using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class AttackChargesInfo : AttackOmniInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new AttackCharges(init.Self, this);
        }
    }
    public class AttackCharges:AttackOmni
    {
        public AttackCharges(Actor self,AttackChargesInfo info) : base(self, info) { }
    }
}
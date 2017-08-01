using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    class NukePowerInfo : SupportPowerInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new NukePower(init.Self, this);
        }
    }

    class NukePower:SupportPower
    {

        public NukePower(Actor self,NukePowerInfo info) : base(self, info) { }
    }
}
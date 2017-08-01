using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class AirstrikePowerInfo : SupportPowerInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new AirstrikePower(init.Self, this);
        }
    }
    public class AirstrikePower:SupportPower
    {
        public AirstrikePower(Actor self,AirstrikePowerInfo info) : base(self, info) { }
    }
}
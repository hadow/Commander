using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{


    public class PowerInfo : UpgradableTraitInfo
    {
        public readonly int Amount = 0;

        public override object Create(ActorInitializer init)
        {
            return new Power(init.Self, this);
        }
    }
    public class Power:UpgradableTrait<PowerInfo>
    {
        public Power(Actor self,PowerInfo info) : base(info) { }
    }
}
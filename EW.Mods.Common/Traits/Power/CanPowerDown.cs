using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class CanPowerDownInfo : UpgradableTraitInfo, Requires<PowerInfo>
    {
        public override object Create(ActorInitializer init)
        {
            return new CanPowerDown(init.Self, this);
        }
    }

    public class CanPowerDown:UpgradableTrait<CanPowerDownInfo>
    {

        public CanPowerDown(Actor self,CanPowerDownInfo info) : base(info) { }
    }
}
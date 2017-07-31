using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class CanPowerDownInfo : UpgradableTraitInfo, Requires<PowerInfo>
    {
        public override object Create(ActorInitializer init)
        {
            throw new NotImplementedException();
        }
    }

    public class CanPowerDown
    {
    }
}
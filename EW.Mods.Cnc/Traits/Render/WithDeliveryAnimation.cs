using System;
using EW.Traits;
using EW.Mods.Common.Traits;
namespace EW.Mods.Cnc.Traits
{


    public class WithDeliveryAnimationInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new WithDeliveryAnimation(); }
    }

    public class WithDeliveryAnimation
    {
    }
}
using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class WithChargeAnimationInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new WithChargeAnimation(); }
    }
    public class WithChargeAnimation
    {
    }
}
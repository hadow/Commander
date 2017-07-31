using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    class SupportPowerChargeBarInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new SupportPowerChargeBar(); }
    }
    class SupportPowerChargeBar
    {
    }
}
using System;
using EW.Traits;
namespace EW.Mods.Common.Traits.Render
{
    class WithChargeOverlayInfo:ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new WithChargeOverlay();
        }
    }

    class WithChargeOverlay
    {

    }
}
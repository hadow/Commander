using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits.Render
{
    class WithHarvestOverlayInfo:ITraitInfo
    {

        public object Create(ActorInitializer init)
        {
            return new WithHarvestOverlay();
        }
    }

    class WithHarvestOverlay
    {

    }
}
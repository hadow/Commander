using System;
using EW.Traits;

namespace EW.Mods.Common.Traits.Render
{
    public class WithBuildingPlacedOverlayInfo:ITraitInfo
    {

        public object Create(ActorInitializer init)
        {
            return new WithBuildingPlacedOverlay();
        }
    }

    public class WithBuildingPlacedOverlay
    {

    }
}
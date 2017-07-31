using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    class WithProductionDoorOverlayInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new WithProductionDoorOverlay(); }
    }
    class WithProductionDoorOverlay
    {
    }
}
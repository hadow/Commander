﻿using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    class CustomTerrainDebugOverlayInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new CustomTerrainDebugOverlay(); }
    }
    class CustomTerrainDebugOverlay
    {
    }
}
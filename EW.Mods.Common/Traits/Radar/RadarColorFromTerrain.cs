using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class RadarColorFromTerrainInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new RadarColorFromTerrain(); }
    }

    public class RadarColorFromTerrain
    {
    }
}
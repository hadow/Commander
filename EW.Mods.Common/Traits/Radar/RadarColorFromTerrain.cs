using System;
using System.Collections.Generic;
using System.Drawing;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class RadarColorFromTerrainInfo : ITraitInfo
    {
        [FieldLoader.Require]
        public readonly string Terrain;

        public object Create(ActorInitializer init) { return new RadarColorFromTerrain(init.Self,Terrain); }
    }

    public class RadarColorFromTerrain:IRadarColorModifier
    {
        Color c;
        public RadarColorFromTerrain(Actor self,string terrain)
        {
            var tileSet = self.World.Map.Rules.TileSet;
            c = tileSet[tileSet.GetTerrainIndex(terrain)].Color;
        }

        public Color RadarColorOverride(Actor self,Color color) { return c; }
    }
}
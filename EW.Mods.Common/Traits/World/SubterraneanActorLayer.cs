using System;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// 地下
    /// </summary>
    public class SubterraneanActorLayerInfo : ITraitInfo
    {

        public readonly string TerrainType = "Subterranean";

        public readonly WDist HeightOffset = -new WDist(2048);

        public readonly int SmoothingRadius = 2;

        public object Create(ActorInitializer init)
        {
            return new SubterraneanActorLayer(init.Self,this);
        }
    }

    public class SubterraneanActorLayer:ICustomMovementLayer
    {
        readonly Map map;

        readonly byte terrainIndex;

        readonly CellLayer<int> height;

        public SubterraneanActorLayer(Actor self,SubterraneanActorLayerInfo info)
        {
            map = self.World.Map;
            terrainIndex = self.World.Map.Rules.TileSet.GetTerrainIndex(info.TerrainType);
            height = new CellLayer<int>(map);

            foreach(var c in map.AllCells)
            {
                var neighbourCount = 0;
                var neighbourHeight = 0;

                for(var dy = -info.SmoothingRadius; dy <= info.SmoothingRadius; dy++)
                {
                    for(var dx = -info.SmoothingRadius; dx <= info.SmoothingRadius; dx++)
                    {
                        var neighbour = c + new CVec(dx, dy);

                        if (!map.AllCells.Contains(neighbour))
                            continue;

                        neighbourCount++;
                        neighbourHeight += map.Height[neighbour];
                    }
                }

                height[c] = info.HeightOffset.Length + neighbourHeight * 512 / neighbourCount;
            }
        }


        bool ICustomMovementLayer.EnabledForActor(ActorInfo ai, MobileInfo mi) { return mi.Subterranean; }

        byte ICustomMovementLayer.Index { get { return CustomMovementLayerType.Subterranean; } }

        bool ICustomMovementLayer.InteractsWithDefaultLayer { get { return false; } }

        WPos ICustomMovementLayer.CenterOfCell(CPos cell)
        {
            var pos = map.CenterOfCell(cell);
            return pos + new WVec(0, 0, height[cell] - pos.Z);
        }

        bool ValidTransitionCell(CPos cell, MobileInfo mi)
        {
            var terrainType = map.GetTerrainInfo(cell).Type;

            if (!mi.SubterraneanTransitionTerrainTypes.Contains(terrainType) && mi.SubterraneanTransitionTerrainTypes.Any())
                return false;

            if (mi.SubterraneanTransitionOnRamps)
                return true;

            var tile = map.Tiles[cell];
            var ti = map.Rules.TileSet.GetTileInfo(tile);
            return ti != null || ti.RampType == 0;


        }

        byte ICustomMovementLayer.GetTerrainIndex(CPos cell) { return terrainIndex; }

        int ICustomMovementLayer.ExitMovementCost(ActorInfo ai, MobileInfo mi, CPos cell)
        {
            return ValidTransitionCell(cell, mi) ? mi.SubterraneanTransitionCost : int.MaxValue;
        }

        int ICustomMovementLayer.EntryMovementCost(ActorInfo ai, MobileInfo mi, CPos cell)
        {
            return ValidTransitionCell(cell, mi) ? mi.SubterraneanTransitionCost : int.MaxValue;
        }

    }
}
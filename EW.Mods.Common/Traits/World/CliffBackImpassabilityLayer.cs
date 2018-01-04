using System;
using System.Linq;
using EW.Traits;
using EW.Graphics;
namespace EW.Mods.Common.Traits
{
    class CliffBackImpassabilityLayerInfo:ITraitInfo
    {
        public readonly string TerrainType = "Impassable";

        public object Create(ActorInitializer init)
        {
            return new CliffBackImpassabilityLayer(this);
        }
    }


    class CliffBackImpassabilityLayer:IWorldLoaded
    {

        readonly CliffBackImpassabilityLayerInfo info;

        public CliffBackImpassabilityLayer(CliffBackImpassabilityLayerInfo info)
        {
            this.info = info;
        }

        public void WorldLoaded(World w,WorldRenderer wr)
        {
            var tileType = w.Map.Rules.TileSet.GetTerrainIndex(info.TerrainType);

            var tunnelPortals = w.WorldActor.Info.TraitInfos<TerrainTunnelInfo>().SelectMany(mti => mti.PortalCells()).ToHashSet();

            foreach(var uv in w.Map.AllCells.MapCoords)
            {
                if (tunnelPortals.Contains(uv.ToCPos(w.Map)))
                    continue;

                var testCells = w.Map.ProjectedCellsCovering(uv).SelectMany(puv => w.Map.Unproject(puv));
                if (testCells.Any(x => x.V >= uv.V + 4))
                    w.Map.CustomTerrain[uv] = tileType;
            }
        }


    }
}
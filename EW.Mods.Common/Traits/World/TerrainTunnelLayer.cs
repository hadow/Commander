using System;
using System.Collections.Generic;
using EW.Graphics;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class TerrainTunnelLayerInfo : ITraitInfo,Requires<DomainIndexInfo>
    {
        [Desc("Terrain type used by cells outside any tunnel footprint.")]
        public readonly string ImpassableTerrainType = "Impassable";

        public object Create(ActorInitializer init)
        {
            return new TerrainTunnelLayer(init.Self,this);
        }
    }

    public class TerrainTunnelLayer:ICustomMovementLayer,IWorldLoaded
    {
        readonly Map map;
        readonly CellLayer<WPos> cellCenters;
        readonly CellLayer<byte> terrainIndices;
        readonly HashSet<CPos> portals = new HashSet<CPos>();
        bool enabled;

        public TerrainTunnelLayer(Actor self,TerrainTunnelLayerInfo info)
        {
            map = self.World.Map;
            cellCenters = new CellLayer<WPos>(map);
            terrainIndices = new CellLayer<byte>(map);
            terrainIndices.Clear(map.Rules.TileSet.GetTerrainIndex(info.ImpassableTerrainType));
        }


        public void WorldLoaded(World world,WorldRenderer wr)
        {
            var domainIndex = world.WorldActor.Trait<DomainIndex>();
            var cellHeight = world.Map.CellHeightStep.Length;
            foreach (var tti in world.WorldActor.Info.TraitInfos<TerrainTunnelInfo>())
            {
                enabled = true;

                var terrain = map.Rules.TileSet.GetTerrainIndex(tti.TerrainType);
                foreach (var c in tti.TunnelCells())
                {
                    var uv = c.ToMPos(map);
                    terrainIndices[uv] = terrain;

                    var pos = map.CenterOfCell(c);
                    cellCenters[uv] = pos - new WVec(0, 0, pos.Z - cellHeight * tti.Height);
                }

                var portal = tti.PortalCells();
                domainIndex.AddFixedConnection(portal);
                foreach (var c in portal)
                {
                    // Need to explicitly set both default and tunnel layers, otherwise the .Contains check will fail
                    portals.Add(new CPos(c.X, c.Y, 0));
                    portals.Add(new CPos(c.X, c.Y, CustomMovementLayerType.Tunnel));
                }
            }
        }

        bool ICustomMovementLayer.EnabledForActor(ActorInfo ai, MobileInfo mi)
        {
            return enabled;
        }

        byte ICustomMovementLayer.Index { get { return CustomMovementLayerType.Tunnel; } }

        bool ICustomMovementLayer.InteractsWithDefaultLayer { get { return false; } }


        WPos ICustomMovementLayer.CenterOfCell(CPos cell)
        {
            return cellCenters[cell];
        }

        int ICustomMovementLayer.EntryMovementCost(ActorInfo ai, MobileInfo mi, CPos cell)
        {
            return portals.Contains(cell) ? 0 : int.MaxValue;
        }

        int ICustomMovementLayer.ExitMovementCost(ActorInfo ai, MobileInfo mi, CPos cell)
        {
            return portals.Contains(cell) ? 0 : int.MaxValue;
        }

        byte ICustomMovementLayer.GetTerrainIndex(CPos cell)
        {
            return terrainIndices[cell];
        }

    }
}
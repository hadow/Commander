using System;
using System.Collections.Generic;
using EW.Graphics;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class TerrainTunnelLayerInfo : ITraitInfo,Requires<DomainIndexInfo>
    {
        public object Create(ActorInitializer init)
        {
            return new TerrainTunnelLayer(init.Self,this);
        }
    }

    public class TerrainTunnelLayer:ICustomMovementLayer,IWorldLoaded
    {
        bool enabled;
        readonly CellLayer<WPos> cellCenters;
        readonly HashSet<CPos> portals = new HashSet<CPos>();
        readonly CellLayer<byte> terrainIndices;
          
        public TerrainTunnelLayer(Actor self,TerrainTunnelLayerInfo info)
        {

        }


        public void WorldLoaded(World world,WorldRenderer wr)
        {

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
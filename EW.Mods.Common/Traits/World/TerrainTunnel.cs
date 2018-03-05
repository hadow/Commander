using System;
using System.Linq;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class TerrainTunnelInfo : TraitInfo<TerrainTunnel>, Requires<TerrainTunnelLayerInfo>
    {
        [FieldLoader.Require]
        [Desc("Location of the tunnel")]
        public readonly CPos Location = CPos.Zero;

        [FieldLoader.Require]
        [Desc("Height of the tunnel floor in map height steps.")]
        public readonly byte Height = 0;

        [FieldLoader.Require]
        [Desc("Size of the tunnel footprint")]
        public readonly CVec Dimensions = CVec.Zero;

        [FieldLoader.Require]
        [Desc("Tunnel footprint.", "_ is passable, x is blocked, and o are tunnel portals.")]
        public readonly string Footprint = string.Empty;

        [FieldLoader.Require]
        [Desc("Terrain type of the tunnel floor.")]
        public readonly string TerrainType = null;

        public IEnumerable<CPos> PortalCells()
        {
            return CellsMatching('0');
        }

        public IEnumerable<CPos> TunnelCells()
        {
            return CellsMatching('_').Concat(CellsMatching('o'));
        }

        IEnumerable<CPos> CellsMatching(char c)
        {
            var index = 0;
            var footprint = Footprint.Where(x => !char.IsWhiteSpace(x)).ToArray();

            for(var y = 0; y < Dimensions.Y; y++)
            {
                for(var x = 0; x < Dimensions.X; x++)
                {
                    if (footprint[index++] == c)
                        yield return Location + new CVec(x, y);
                }
            }
        }
    }



    public class TerrainTunnel{}
}
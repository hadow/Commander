using System;
using System.Linq;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class TerrainTunnelInfo : TraitInfo<TerrainTunnel>, Requires<TerrainTunnelLayerInfo>
    {
        public readonly CPos Location = CPos.Zero;

        public readonly CVec Dimensions = CVec.Zero;

        public readonly string Footprint = string.Empty;

        public IEnumerable<CPos> PortalCells()
        {
            return CellsMatching('0');
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



    public class TerrainTunnel
    {
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EW.Traits;
namespace EW
{
    public enum MapGridT
    {
        Rectangular,
        RectangularIsometric
    }

    /// <summary>
    /// 
    /// </summary>
    public class MapGrid:IGlobalModData
    {
        public readonly MapGridT Type = MapGridT.Rectangular;
        public readonly Size TileSize = new Size(24, 24);

        public readonly bool EnableDepthBuffer = false;

        public readonly SubCell DefaultSubCell = (SubCell)byte.MaxValue;

        public readonly int MaximumTileSearchRange = 50;
        

        /// <summary>
        /// 最大地形高度
        /// </summary>
        public readonly byte MaximumTerrainHeight = 0;

        public MapGrid(MiniYaml yaml)
        {
            FieldLoader.Load(this, yaml);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        CVec[][] CreateTilesByDistance()
        {
            var ts = new List<CVec>[MaximumTileSearchRange + 1];
            for (var i = 0; i < MaximumTileSearchRange + 1; i++)
                ts[i] = new List<CVec>();


            for(var j = -MaximumTileSearchRange; j <=MaximumTileSearchRange; j++)
            {
                for(var i = -MaximumTileSearchRange; i <= MaximumTileSearchRange; i++)
                {
                    if(MaximumTileSearchRange * MaximumTileSearchRange >= i * i + j * j)
                    {
                        ts[Exts.ISqrt(i * i + j * j, Exts.ISqrtRoundMode.Ceiling)].Add(new CVec(i, j));
                    }
                }
            }

            foreach(var list in ts)
            {
                list.Sort((a, b) => {

                    var result = a.LengthSquard.CompareTo(b.LengthSquard);
                    if (result != 0)
                        return result;

                    result = a.GetHashCode().CompareTo(b.GetHashCode());
                    if (result != 0)
                        return result;

                    result = a.X.CompareTo(b.X);
                    if (result != 0)
                        return result;

                    return a.Y.CompareTo(b.Y);
                });
            }

            return ts.Select(list => list.ToArray()).ToArray();
        }
    }
}
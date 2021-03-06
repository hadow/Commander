using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using EW.Traits;
namespace EW
{
    public enum MapGridT
    {
        Rectangular,
        RectangularIsometric//等距
    }

    /// <summary>
    /// 地图网格
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

        public WVec[][] CellCorners { get; private set; }

        internal readonly CVec[][] TilesByDistance;

        public readonly WVec[] SubCellOffsets =
        {
            new WVec(0,0,0),        //full cell - index 0
            new WVec(-299,-256,0),  //top left - index 1
            new WVec(256,-256,0),   //top right -index 2
            new WVec(0,0,0),        //center -index 3
            new WVec(-299,256,0),   //bottom left -index 4
            new WVec(256,256,0),    //bottom right -index 5
        };

        readonly int[][] cellCornerHalfHeights = new int[][]
        {
            //Flat
            new[]{0,0,0,0},

            //Slopes (two corners high)
            new[]{0,0,1,1},
            new[]{1,0,0,1},
            new[]{1,1,0,0},
            new[]{0,1,1,0},

            //Slopes (one corner high)
            new[]{0,0,0,1},
            new[]{1,0,0,0},
            new[]{0,1,0,0},
            new[]{0,0,1,0},

            //Slopes (three corner high)
            new[]{1,0,1,1},
            new[]{1,1,0,1},
            new[]{1,1,1,0},
            new[]{0,1,1,1},

            //Slopes (two corners high, one corner double high)
            new[]{1,0,1,2},
            new[]{2,1,0,1},
            new[]{1,2,1,0},
            new[]{0,1,2,1},

            //Slopes (two corners high,alternating)
            new[]{1,0,1,0},
            new[]{0,1,0,1},
            new[]{1,0,1,0},
            new[]{0,1,0,1},
        };


        public MapGrid(MiniYaml yaml)
        {
            FieldLoader.Load(this, yaml);

            // The default subcell index defaults to the middle entry 默认子单元格索为中间条目
            var defaultSubCellIndex = (byte)DefaultSubCell;
            if (defaultSubCellIndex == byte.MaxValue)
                DefaultSubCell = (SubCell)(SubCellOffsets.Length / 2);
            
            else if (defaultSubCellIndex < (SubCellOffsets.Length > 1 ? 1 : 0) || defaultSubCellIndex >= SubCellOffsets.Length)
                throw new InvalidDataException("Subcell default index must be a valid index into the offset triples and must be greater than 0 for mods with subcells.");

            //var leftDelta = Type == MapGridT.RectangularIsometric ? new WVec(-512, 0, 0) : new WVec(-512, -512, 0);
            //var topDelta = Type == MapGridT.RectangularIsometric ? new WVec(0, -512, 0) : new WVec(512, -512, 0);
            //var rightDelta = Type == MapGridT.RectangularIsometric ? new WVec(512, 0, 0) : new WVec(512, 512, 0);
            //var bottomDelta = Type == MapGridT.RectangularIsometric ? new WVec(0, 512, 0) : new WVec(-512, 512, 0);

            //CellCorners = cellCornerHalfHeights.Select(ramp => new WVec[]
            //{
            //    leftDelta + new WVec(0,0,512*ramp[0]),
            //    topDelta + new WVec(0,0,512*ramp[1]),
            //    rightDelta + new WVec(0,0,512*ramp[2]),
            //    bottomDelta+new WVec(0,0,512*ramp[3])

            //}).ToArray();

            var makeCorners = Type == MapGridT.Rectangular ? (Func<int[],WVec[]>)RectangularCellCorners : IsometricCellCorners;

            CellCorners = cellCornerHalfHeights.Select(makeCorners).ToArray();

            TilesByDistance = CreateTilesByDistance();
        }

        static WVec[] IsometricCellCorners(int[] cornerHeight){

            return new WVec[]{
                new WVec(-724,0,724*cornerHeight[0]),
                new WVec(0,-724,724*cornerHeight[1]),
                new WVec(724,0,724*cornerHeight[2]),
                new WVec(0,724,724*cornerHeight[3])
            };
        }

        static WVec[] RectangularCellCorners(int[] cornerHeight){
            return new WVec[]{
                new WVec(-512,-512,512*cornerHeight[0]),
                new WVec(512,-512,512*cornerHeight[1]),
                new WVec(512,512,512*cornerHeight[2]),
                new WVec(-512,512,512*cornerHeight[3]),
            };
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
            //Sort each integer-distance group by the actual distance.
            foreach(var list in ts)
            {
                list.Sort((a, b) => {

                    var result = a.LengthSquard.CompareTo(b.LengthSquard);
                    if (result != 0)
                        return result;


                    //如果长度相等，则使用其他方法对其进行排序。首先尝试哈希码，因为它会给出比X或Y更多的随机出现结果，总是偏向最左边 / 最上面的位置。
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

        public WVec OffsetOfSubCell(SubCell subCell)
        {
            if (subCell == SubCell.Invalid || subCell == SubCell.Any)
                return WVec.Zero;

            return SubCellOffsets[(int)subCell];
        }
    }
}
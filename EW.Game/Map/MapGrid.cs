using System;
using System.Collections.Generic;
using System.Drawing;
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
        /// <summary>
        /// 最大地形高度
        /// </summary>
        public readonly byte MaximumTerrainHeight = 0;
    }
}
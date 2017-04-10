using System;
using System.Collections.Generic;

namespace EW
{
    public enum MapGridT
    {
        Rectangular,
        RectangularIsometric
    }
    public class MapGrid
    {
        public readonly MapGridT Type = MapGridT.Rectangular;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace EW
{
    /// <summary>
    /// Represents a (on-screen) rectangular collection of tiles.
    /// TopLeft and BottomRight are inclusive
    /// </summary>
    public class CellRegion:IEnumerable<CPos>
    {
        /// <summary>
        /// Corners of the region 
        /// </summary>
        public readonly CPos TopLeft;
        public readonly CPos BottomRight;

        readonly MapGridT gridT;

        /// <summary>
        /// 
        /// </summary>
        readonly MPos mapTopLeft;
        readonly MPos mapBottomRight;

        public CellRegion(MapGridT gridT,CPos topLeft,CPos bottomRight)
        {
            this.gridT = gridT;

            this.TopLeft = topLeft;
            this.BottomRight = bottomRight;

            mapTopLeft = TopLeft.ToMPos(gridT);
            mapBottomRight = BottomRight.ToMPos(gridT);
            
        }

        public bool Contains(CPos cell)
        {
            var uv = cell.ToMPos(gridT);
            return uv.U >= mapTopLeft.U && uv.U <= mapBottomRight.U && uv.V >= mapTopLeft.V && uv.V <= mapBottomRight.V;
        }

        /// <summary>
        /// 为了提供性能，新增枚举map-coords
        /// </summary>
        public MapCoordsRegion MapCoords
        {
            get { return new MapCoordsRegion(mapTopLeft, mapBottomRight); }
        }

        public CellRegionEnumerator GetEnumerator()
        {
            return new CellRegionEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<CPos> IEnumerable<CPos>.GetEnumerator()
        {
            return GetEnumerator();
        }


        public sealed class CellRegionEnumerator : IEnumerator<CPos>
        {
            readonly CellRegion cr;

            //current position in map coordinates
            int u, v;

            //current position in cell coordinates
            CPos current;

            public CellRegionEnumerator(CellRegion region)
            {
                cr = region;
                Reset();
            }

            public bool MoveNext()
            {
                u += 1;
                if(u > cr.mapBottomRight.U)
                {
                    v += 1;
                    u = cr.mapTopLeft.U;

                    if (v > cr.mapBottomRight.V)
                        return false;
                }
                current = new MPos(u, v).ToCPos(cr.gridT);
                return true;
            }

            public void Reset()
            {
                u = cr.mapTopLeft.U - 1;
                v = cr.mapTopLeft.V;
            }

            public CPos Current { get { return current; } }

            object IEnumerator.Current { get { return Current; } }

            public void Dispose() { }
        }

    }
}
using System;
using System.Collections;
using System.Collections.Generic;
namespace EW
{
    /// <summary>
    /// 
    /// </summary>
    public class ProjectedCellRegion:IEnumerable<PPos>
    {

        //Corner of the region
        public readonly PPos TopLeft;
        public readonly PPos BottomRight;

        /// <summary>
        /// 该投影地图区域内包含的所有单元格
        /// </summary>
        readonly MPos mapTopLef;
        readonly MPos mapBottomRight;

        public ProjectedCellRegion(Map map,PPos topLeft,PPos bottomRight)
        {
            TopLeft = topLeft;
            BottomRight = bottomRight;

            mapTopLef = (MPos)topLeft;

            var maxHeight = map.Grid.MaximumTerrainHeight;

            var heightOffset = map.Grid.Type == MapGridT.RectangularIsometric ? maxHeight : maxHeight / 2;
            mapBottomRight = map.Height.Clamp(new MPos(bottomRight.U, bottomRight.V + heightOffset));
        }

        public ProjectedCellRegionEnumerator GetEnumerator()
        {
            return new ProjectedCellRegionEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<PPos> IEnumerable<PPos>.GetEnumerator()
        {
            return GetEnumerator();
        }


        /// <summary>
        /// 
        /// </summary>
        public sealed class ProjectedCellRegionEnumerator : IEnumerator<PPos>
        {
            readonly ProjectedCellRegion r;

            int u, v;

            PPos current;

            public ProjectedCellRegionEnumerator(ProjectedCellRegion region)
            {
                r = region;
                Reset();
            }

            public bool MoveNext()
            {
                u += 1;
                //检查列溢出
                if (u > r.BottomRight.U)
                {
                    v += 1;
                    u = r.TopLeft.U;
                    //检查 行溢出
                    if (v > r.BottomRight.V)
                        return false;
                }
                current = new PPos(u, v);
                return true;
            }
            public void Reset()
            {
                u = r.TopLeft.U - 1;
                v = r.TopLeft.V;
            }

            public PPos Current { get { return current; } }

            object IEnumerator.Current { get { return Current; } }

            public void Dispose() { }
        }
    }
}
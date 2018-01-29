using System;
using System.Collections;
using System.Collections.Generic;
namespace EW
{
    /// <summary>
    /// 投射范围单元格
    /// </summary>
    public class ProjectedCellRegion:IEnumerable<PPos>
    {

        //Corner of the region
        public readonly PPos TopLeft;
        public readonly PPos BottomRight;

        /// <summary>
        /// 该投影地图区域内包含的所有单元格,理论上应该被投射在此区域
        /// </summary>
        readonly MPos mapTopLef;
        readonly MPos mapBottomRight;

        public ProjectedCellRegion(Map map,PPos topLeft,PPos bottomRight)
        {
            TopLeft = topLeft;
            BottomRight = bottomRight;

            //The projection from MPos->PPos cannot produce a larger V coordinate
            //so the top edge of the MPos region is the same as the PPos region.
            //(in fact the cells are identical if height ==0)
            //MPos -> PPos 的投影不能产生较大的V 坐标，因此MPos区域内的顶边与 PPos 相同(事实上，如果height == 0,单元格是一样的)
            mapTopLef = (MPos)topLeft;

            var maxHeight = map.Grid.MaximumTerrainHeight;
            var heightOffset = map.Grid.Type == MapGridT.RectangularIsometric ? maxHeight : maxHeight / 2;

            //Use the map Height data array to clamp the bottom coordinate so it doesn't overflow the map
            //使用地图高度数据数组来限制底部坐标，使其不会溢出地图
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
        /// 地图坐标系中包含可能投影在该区域内的所有单元格信息
        /// 为了提高性能，不会验证各个地图单元格是否真正应在当前区域内投影
        /// </summary>
        public MapCoordsRegion CandidateMapCoords { get { return new MapCoordsRegion(mapTopLef, mapBottomRight); } }


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
using System;
using System.Linq;
using System.Collections.Generic;
using EW.Xna.Platforms;


namespace EW.Graphics
{

    /// <summary>
    /// 游戏视窗口
    /// </summary>
    public class GameViewPort
    {

        Point viewportSize;
        readonly float[] availableZoomSteps = new[] { 2f, 1f, 0.5f, 0.25f };

        public float[] AvailableZoomSteps
        {
            get { return availableZoomSteps; }
        }
        readonly WorldRenderer worldRenderer;

        /// <summary>
        /// Map Bounds(world-px)
        /// </summary>
        readonly Rectangle mapBounds;

        readonly System.Drawing.Size tileSize;

        public Point CenterLocation { get; private set; }

        public WPos CenterPosition { get { return worldRenderer.ProjectedPosition(CenterLocation); } }

        public Point TopLeft { get { return CenterLocation - viewportSize / 2; } }

        public Point BottomRight { get { return CenterLocation + viewportSize / 2; } }

        float zoom = 1f;

        bool allCellsDirty = true;
        bool cellsDirty = true;

        ProjectedCellRegion cells;

        ProjectedCellRegion allCells;

        /// <summary>
        /// 
        /// </summary>
        public float Zoom
        {
            get
            {
                return zoom;
            }
            set
            {
                var newValue = ClosestTo(AvailableZoomSteps, value);
                zoom = newValue;
                allCellsDirty = true;
                cellsDirty = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        float ClosestTo(float[] collection,float target)
        {
            var closestValue = collection.First();

            //Todo

            return closestValue;
        }
        public GameViewPort(WorldRenderer wr,Map map)
        {
            worldRenderer = wr;

            var grid = WarGame.ModData.Manifest.Get<MapGrid>();

            if(wr.World.Type == WorldT.Editor)
            {

            }
            else
            {
                var tl = wr.ScreenPxPosition(map.ProjectedTopLeft);
                var br = wr.ScreenPxPosition(map.ProjectedBottomRight);
                mapBounds = Rectangle.FromLTRB(tl.X, tl.Y, br.X, br.Y);
                CenterLocation = (tl + br) / 2;
            }

            tileSize = grid.TileSize;
        }

        public ProjectedCellRegion VisibleCellsInsideBounds
        {
            get
            {
                if (cellsDirty)
                {
                    cells = CalculateVisibleCells(false);
                    cellsDirty = false;
                }
                return cells;
            }
        }

        public ProjectedCellRegion AllVisibleCells
        {
            get
            {
                if (allCellsDirty)
                {
                    allCells = CalculateVisibleCells(false);
                    allCellsDirty = false;
                }

                return allCells;
            }
        }

        /// <summary>
        /// 计算可视单元格
        /// </summary>
        /// <param name="insideBounds"></param>
        /// <returns></returns>
        ProjectedCellRegion CalculateVisibleCells(bool insideBounds)
        {
            var map = worldRenderer.World.Map;

            var tl = (PPos)map.CellContaining(worldRenderer.ProjectedPosition(TopLeft)).ToMPos(map);
            var br = (PPos)map.CellContaining(worldRenderer.ProjectedPosition(BottomRight)).ToMPos(map);

            if(map.Grid.Type == MapGridT.RectangularIsometric)
            {
                tl = new PPos(tl.U - 1,tl.V-1);
                br = new PPos(br.U + 1, br.V + 1);
            }

            if (insideBounds)
            {
                tl = map.Clamp(tl);
                br = map.Clamp(br);
            }

            return new ProjectedCellRegion(map, tl, br);
        }
    }
}
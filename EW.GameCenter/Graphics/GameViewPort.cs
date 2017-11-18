using System;
using System.Linq;
using EW.Xna.Platforms;
using EW.Primitives;

namespace EW.Graphics
{

    /// <summary>
    /// 游戏视窗口
    /// </summary>
    public class GameViewPort:GameComponent
    {

        Int2 viewportSize;
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


        //Viewport geometry (world-px)
        public Int2 CenterLocation { get; private set; }

        public WPos CenterPosition { get { return worldRenderer.ProjectedPosition(CenterLocation); } }
        //视窗左上角
        public Int2 TopLeft { get { return CenterLocation - viewportSize / 2; } }

        //视窗右下角
        public Int2 BottomRight { get { return CenterLocation + viewportSize / 2; } }

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
                viewportSize = ((1f / zoom) * new Vector2(this.Game.GraphicsDevice.DisplayMode.Width, this.Game.GraphicsDevice.DisplayMode.Height)).ToInt2();
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

            var subtractResult = Math.Abs(closestValue - target);

            foreach(var element in collection)
            {
                if (Math.Abs(element - target) < subtractResult)
                {
                    subtractResult = Math.Abs(element - target);
                    closestValue = element;
                }
            }
            return closestValue;
        }
        public GameViewPort(Game game,WorldRenderer wr,Map map):base(game)
        {
            worldRenderer = wr;
            ScreenClip = Rectangle.FromLTRB(0, 0, Game.GraphicsDevice.DisplayMode.Width, Game.GraphicsDevice.DisplayMode.Height);
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
            Zoom = WarGame.Settings.Graphics.PixelDouble ? 2 : 1;
            tileSize = grid.TileSize;
        }

        public ProjectedCellRegion VisibleCellsInsideBounds
        {
            get
            {
                if (cellsDirty)
                {
                    cells = CalculateVisibleCells(true);
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
        /// 计算可视范围内单元格
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


        //Rectangle(in viewport coords) that contains things to be drawn
        //static readonly Rectangle ScreenClip = Rectangle.FromLTRB(0,0,GraphicsDeviceManager.M.GraphicsDevice.DisplayMode.Width,GraphicsDeviceManager.M.GraphicsDevice.DisplayMode.Height);
        //readonly Rectangle ScreenClip = Rectangle.FromLTRB(0, 0, Game.GraphicsDevice.DisplayMode.Width, 0);
        readonly Rectangle ScreenClip;

        /// <summary>
        /// Rectangle(int viewport coords) that contains things to be drawn
        /// </summary>
        /// <param name="insideBounds"></param>
        /// <returns></returns>
        public Rectangle GetScissorBounds(bool insideBounds)
        {
            //Visible rectangle in world coordinates (expanded to the corner of the cells)
            var bounds = insideBounds ? VisibleCellsInsideBounds : AllVisibleCells;
            var map = worldRenderer.World.Map;
            
            //mpos -> cpos -> wpos
            var ctl = map.CenterOfCell(((MPos)bounds.TopLeft).ToCPos(map)) - new WVec(512, 512, 0);
            var cbr = map.CenterOfCell(((MPos)bounds.BottomRight).ToCPos(map)) + new WVec(512, 512, 0);

            //Convert to screen coordinates
            var tl = WorldToViewPx(worldRenderer.ScreenPxPosition(ctl - new WVec(0, 0, ctl.Z))).Clamp(ScreenClip);
            var br = WorldToViewPx(worldRenderer.ScreenPxPosition(cbr - new WVec(0, 0, cbr.Z))).Clamp(ScreenClip);

            return Rectangle.FromLTRB(tl.X - tileSize.Width, tl.Y - tileSize.Height, br.X + tileSize.Width, br.Y + tileSize.Height);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="ignoreBorders"></param>
        public void Scroll(Vector2 delta,bool ignoreBorders)
        {
            CenterLocation += (1f / Zoom * delta).ToInt2();
            cellsDirty = true;
            allCellsDirty = true;

        }

        /// <summary>
        /// 世界屏幕坐标-> 视窗口坐标
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        public Int2 WorldToViewPx(Int2 world)
        {
            return (Zoom * (world - TopLeft).ToVector2()).ToInt2();
        }

        /// <summary>
        /// 视窗口坐标->世界屏幕坐标
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public Int2 ViewToWorldPx(Int2 view)
        {
            return (1f / Zoom * view.ToVector2()).ToInt2() + TopLeft;
        }

        /// <summary>
        /// 聚焦中心点
        /// </summary>
        /// <param name="pos">世界坐标点</param>
        public void Center(WPos pos)
        {
            CenterLocation = worldRenderer.ScreenPxPosition(pos).Clamp(mapBounds);
            cellsDirty = true;
            allCellsDirty = true;
        }
    }
}
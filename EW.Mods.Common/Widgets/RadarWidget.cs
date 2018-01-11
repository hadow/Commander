using System;
using System.Drawing;
using System.Linq;
using EW.Widgets;
using EW.Graphics;
using EW.Traits;
using EW.Mods.Common.Traits;
using EW.Framework;
namespace EW.Mods.Common.Widgets
{
    public sealed class RadarWidget:Widget,IDisposable
    {
        readonly World world;
        readonly WorldRenderer worldRenderer;
        readonly RadarPings radarPings;

        readonly bool isRectangularIsometric;

        float previewScale = 0;
        Int2 previewOrigin = Int2.Zero;
        Rectangle mapRect = Rectangle.Empty;


        Sheet radarSheet;
        byte[] radarData;

        Sprite terrainSprite;
        Sprite actorSprite;
        Sprite shroudSprite;
        Shroud renderShroud;

        readonly int cellWidth;
        readonly int previewWidth;
        readonly int previewHeight;


        float radarMinimapHeight;
        int frame;
        bool hasRadar;
        bool cachedEnabled;

        public RadarWidget(World world,WorldRenderer worldRenderer)
        {
            this.world = world;
            this.worldRenderer = worldRenderer;
            radarPings = world.WorldActor.TraitOrDefault<RadarPings>();

            isRectangularIsometric = world.Map.Grid.Type == MapGridT.RectangularIsometric;
            cellWidth = isRectangularIsometric ? 2 : 1;
            previewWidth = world.Map.MapSize.X;
            previewHeight = world.Map.MapSize.Y;

            if (isRectangularIsometric)
                previewWidth = 2 * previewWidth - 1;
        }


        public override void Initialize(WidgetArgs args)
        {
            base.Initialize(args);


            radarSheet = new Sheet(SheetT.BGRA, new Size(2 * previewWidth, 2 * previewHeight).NextPowerOf2());
            radarSheet.CreateBuffer();
            radarData = radarSheet.GetData();

            MapBoundsChanged();

            //Set initial terrain data
            foreach (var cell in world.Map.AllCells)
                UpdateTerrainCell(cell);

            world.Map.Tiles.CellEntryChanged += UpdateTerrainCell;
            world.Map.Tiles.CellEntryChanged += UpdateTerrainCell;

        }

        void MapBoundsChanged()
        {
            var map = world.Map;
            
            // The minimap is drawn in cell space,so we need to unproject the bounds to find the extent of the map.
            var projectedLeft = map.Bounds.Left;
            var projectedRight = map.Bounds.Right;
            var projectedTop = map.Bounds.Top;
            var projectedBottom = map.Bounds.Bottom;

            var top = int.MaxValue;
            var bottom = int.MinValue;
            var left = map.Bounds.Left * cellWidth;
            var right = map.Bounds.Right * cellWidth;

            for(var x = projectedLeft; x < projectedRight; x++)
            {
                var allTop = map.Unproject(new PPos(x, projectedTop));
                var allBottom = map.Unproject(new PPos(x, projectedBottom));
                if (allTop.Any())
                    top = Math.Min(top, allTop.MinBy(uv => uv.V).V);

                if (allBottom.Any())
                    bottom = Math.Max(bottom, allBottom.MinBy(uv => uv.V).V);
            }


            var b = Rectangle.FromLTRB(left, top, right, bottom);
            var rb = RenderBounds;

            previewScale = Math.Min(rb.Width * 1f / b.Width, rb.Height * 1f / b.Height);
            previewOrigin = new Int2((int)((rb.Width - previewScale * b.Width) / 2), (int)((rb.Height - previewScale * b.Height) / 2));
            mapRect = new Rectangle(previewOrigin.X, previewOrigin.Y, (int)(previewScale * b.Width), (int)(previewScale * b.Height));

            terrainSprite = new Sprite(radarSheet, b, TextureChannel.Alpha);
            shroudSprite = new Sprite(radarSheet, new Rectangle(b.Location + new Size(previewWidth, 0), b.Size), TextureChannel.Alpha);
            actorSprite = new Sprite(radarSheet, new Rectangle(b.Location + new Size(0, previewHeight), b.Size), TextureChannel.Alpha);

        }

        void UpdateTerrainCell(CPos cell)
        {
            var uv = cell.ToMPos(world.Map);

            if (!world.Map.CustomTerrain.Contains(uv))
                return;


        }


        public override void Tick()
        {

        }



        public override void Draw()
        {
            if (world == null)
                return;

            if(renderShroud != null)
            {

            }

            radarSheet.CommitBufferedData();

            var o = new Vector2(mapRect.Location.X, mapRect.Location.Y + world.Map.Bounds.Height * previewScale * (1 - radarMinimapHeight) / 2);
            var s = new Vector2(mapRect.Size.Width, mapRect.Size.Height * radarMinimapHeight);

            var rsr = WarGame.Renderer.RgbaSpriteRenderer;
            rsr.DrawSprite(terrainSprite, o, s);
            rsr.DrawSprite(actorSprite, o, s);

            if (renderShroud != null)
                rsr.DrawSprite(shroudSprite, o, s);

            if (hasRadar)
            {
                var tl = CellToMinimapPixel(world.Map.CellContaining(worldRenderer.ProjectedPosition(worldRenderer.ViewPort.TopLeft)));
                var br = CellToMinimapPixel(world.Map.CellContaining(worldRenderer.ProjectedPosition(worldRenderer.ViewPort.BottomRight)));

                WarGame.Renderer.EnableScissor(mapRect);
                DrawRadarPings();
                WarGame.Renderer.RgbaColorRenderer.DrawRect(tl, br, 1, Color.White);
                WarGame.Renderer.DisableScissor();
            }
        }


        void DrawRadarPings()
        {
            if (radarPings == null)
                return;

            foreach(var radarPing in radarPings.Pings.Where(e => e.IsVisible()))
            {
                var c = radarPing.Color;
                var pingCell = world.Map.CellContaining(radarPing.Position);
                var points = radarPing.Points(CellToMinimapPixel(pingCell)).ToArray();
                WarGame.Renderer.RgbaColorRenderer.DrawPolygon(points, 2, c);
            }
        }

        Int2 CellToMinimapPixel(CPos p)
        {
            var uv = p.ToMPos(world.Map);
            var dx = (int)(previewScale * cellWidth * (uv.U - world.Map.Bounds.Left));
            var dy = (int)(previewScale * (uv.V - world.Map.Bounds.Top));

            //Odd rows are shifted right by 1px
            if (isRectangularIsometric && (uv.V & 1) == 1)
                dx += 1;

            return new Int2(mapRect.X + dx, mapRect.Y + dy);
        }

        


        public void Dispose()
        {
            radarSheet.Dispose();
        }
    }
}
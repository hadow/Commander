using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EW.Widgets;
using EW.Graphics;
using EW.Traits;
using EW.Mods.Common.Traits;
using EW.Framework;
using EW.Framework.Touch;
using EW.Primitives;
namespace EW.Mods.Common.Widgets
{
    public sealed class RadarWidget:Widget,IDisposable
    {
        public string WorldInteractionController = null;
        readonly World world;
        readonly WorldRenderer worldRenderer;
        readonly RadarPings radarPings;

        public Func<bool> IsEnabled = () => true;
        public Action<float> Animating = _ => { };

        public Action AfterOpen = () => { };
        public Action AfterClose = () => { };
        public int AnimationLength = 5;
        public string RadarOnlineSound = null;
        public string RadarOfflineSound = null;
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

        readonly HashSet<PPos> dirtyShroudCells = new HashSet<PPos>();

        [ObjectCreator.UseCtor]
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

        public override bool HandleInput(GestureSample gs)
        {
            if (!mapRect.Contains(gs.Position.ToInt2()))
                return false;

            if (!hasRadar)
                return true;

            var cell = MinimapPixelToCell(gs.Position.ToInt2());
            var pos = world.Map.CenterOfCell(cell);
            if (gs.GestureType == GestureType.FreeDrag)
                worldRenderer.ViewPort.Center(pos);

            return true;
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

        


        public override void Tick()
        {
            //Enable / Disable the radar
            var enabled = IsEnabled();
            if (enabled != cachedEnabled)
                WarGame.Sound.Play(SoundType.UI, enabled ? RadarOnlineSound : RadarOfflineSound);

            cachedEnabled = enabled;

            if (enabled)
            {
                var rp = world.RenderPlayer;
                var newRenderShroud = rp != null ? rp.Shroud : null;
                if(newRenderShroud != renderShroud)
                {
                    if (renderShroud != null)
                        renderShroud.CellsChanged -= MarkShroudDirty;

                    if(newRenderShroud != null)
                    {
                        //Redraw the full shroud sprite
                        MarkShroudDirty(world.Map.AllCells.MapCoords.Select(uv => (PPos)uv));

                        //Update the notification binding
                        newRenderShroud.CellsChanged += MarkShroudDirty;
                    }

                    renderShroud = newRenderShroud;
                }


                //The actor layer is updated every tick
                var stride = radarSheet.Size.Width;

                Array.Clear(radarData, 4 * actorSprite.Bounds.Top * stride, 4 * actorSprite.Bounds.Height * stride);

                var cells = new List<Pair<CPos, Color>>();

                unsafe
                {
                    fixed(byte* colorBytes = &radarData[0])
                    {
                        var colors = (int*)colorBytes;

                        foreach(var t in world.ActorsWithTrait<IRadarSignature>())
                        {
                            if (!t.Actor.IsInWorld || world.FogObscures(t.Actor))
                                continue;

                            cells.Clear();
                            t.Trait.PopulateRadarSignatureCells(t.Actor, cells);

                            foreach(var cell in cells)
                            {
                                if (!world.Map.Contains(cell.First))
                                    continue;

                                var uv = cell.First.ToMPos(world.Map.Grid.Type);
                                var color = cell.Second.ToArgb();

                                if (isRectangularIsometric)
                                {
                                    //Odd rows are shifted right by 1px
                                    var dx = uv.V & 1;
                                    if (uv.U + dx > 0)
                                        colors[(uv.V + previewHeight) * stride + 2 * uv.U + dx - 1] = color;

                                    if (2 * uv.U + dx < stride)
                                        colors[(uv.V + previewHeight) * stride + 2 * uv.U + dx] = color;

                                }
                                else
                                {
                                    colors[(uv.V + previewHeight) * stride + uv.U] = color;
                                }
                            }
                        }
                    }
                }
            }

            var targetFrame = enabled ? AnimationLength : 0;
            hasRadar = enabled && frame == AnimationLength;

            if (frame == targetFrame)
                return;

            frame += enabled ? 1 : -1;
            radarMinimapHeight = Vector2.Lerp(0, 1, (float)frame / AnimationLength);
            Animating(frame * 1f / AnimationLength);

            //Update map rectangle for event handling
            var ro = RenderOrigin;
            mapRect = new Rectangle(previewOrigin.X + ro.X, previewOrigin.Y + ro.Y, mapRect.Width, mapRect.Height);

            //Animation is complete
            if(frame == targetFrame)
            {
                if (enabled)
                    AfterOpen();
                else
                    AfterClose();
            }
           
        }

        void MarkShroudDirty(IEnumerable<PPos> projectedCellsChanged)
        {
            //PERF:Many cells in the shroud change every tick.We only track the changes here and defer the real work
            //we need to do until we render,This allows us to avoid wasted work.
            dirtyShroudCells.UnionWith(projectedCellsChanged);
        }



        public override void Draw()
        {
            if (world == null)
                return;

            if(renderShroud != null)
            {
                foreach (var cell in dirtyShroudCells)
                    UpdateShroudCell(cell);

                dirtyShroudCells.Clear();
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

        void UpdateShroudCell(PPos puv)
        {
            var color = 0;
            var rp = world.RenderPlayer;
            if (rp != null)
            {
                if (!rp.Shroud.IsExplored(puv))
                    color = Color.Black.ToArgb();
                else if (!rp.Shroud.IsVisible(puv))
                    color = Color.FromArgb(128, Color.Black).ToArgb();

            }

            var stride = radarSheet.Size.Width;
            unsafe
            {
                fixed(byte* colorBytes = &radarData[0])
                {
                    var colors = (int*)colorBytes;

                    foreach(var uv in world.Map.Unproject(puv))
                    {
                        if (isRectangularIsometric)
                        {
                            var dx = uv.V & 1;
                            if (uv.U + dx > 0)
                                colors[uv.V * stride + 2 * uv.U + dx - 1 + previewWidth] = color;

                            if (2 * uv.U + dx < stride)
                                colors[uv.V * stride + 2 * uv.U + dx + previewWidth] = color;
                        }
                        else
                            colors[uv.V * stride + uv.U + previewWidth] = color;
                    }
                }
            }
        }

        void UpdateTerrainCell(CPos cell)
        {
            var uv = cell.ToMPos(world.Map);

            if (!world.Map.CustomTerrain.Contains(uv))
                return;

            var custom = world.Map.CustomTerrain[uv];
            int leftColor, rightColor;
            if (custom == byte.MaxValue)
            {
                var type = world.Map.Rules.TileSet.GetTileInfo(world.Map.Tiles[uv]);
                leftColor = type != null ? type.LeftColor.ToArgb() : Color.Black.ToArgb();
                rightColor = type != null ? type.RightColor.ToArgb() : Color.Black.ToArgb();
            }
            else
                leftColor = rightColor = world.Map.Rules.TileSet[custom].Color.ToArgb();

            var stride = radarSheet.Size.Width;

            unsafe
            {
                fixed (byte* colorBytes = &radarData[0])
                {
                    var colors = (int*)colorBytes;
                    if (isRectangularIsometric)
                    {
                        //Odd rows are shifted right by 1px
                        var dx = uv.V & 1;
                        if (uv.U + dx > 0)
                            colors[uv.V * stride + 2 * uv.U + dx - 1] = leftColor;

                        if (2 * uv.U + dx < stride)
                            colors[uv.V * stride + 2 * uv.U + dx] = rightColor;

                    }
                    else
                        colors[uv.V * stride + uv.U] = leftColor;
                }
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

        /// <summary>
        /// 小地图位置转换到世界地图单元格
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        CPos MinimapPixelToCell(Int2 p)
        {
            var u = (int)((p.X - mapRect.X) / (previewScale * cellWidth)) + world.Map.Bounds.Left;
            var v = (int)((p.Y - mapRect.Y) / previewScale) + world.Map.Bounds.Top;

            return new MPos(u, v).ToCPos(world.Map);
        }

        /// <summary>
        /// 单元格转换至小地图像素位置
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
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
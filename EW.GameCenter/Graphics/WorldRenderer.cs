using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EW.Traits;
using EW.Primitives;
using EW.Effects;
using EW.NetWork;
using EW.OpenGLES;
namespace EW.Graphics
{
    /// <summary>
    /// 世界渲染器
    /// </summary>
    public sealed class WorldRenderer
    {
        public static readonly Func<IRenderable, int> RenderableScreenZPositionComparisonKey = r => ZPosition(r.Pos, r.ZOffset);

        public GameViewPort ViewPort { get; private set; }

        public readonly Size TileSize;

        public readonly int TileScale;

        public readonly World World;

        public readonly Theater Theater;

        readonly TerrainRenderer terrainRenderer;

        readonly Dictionary<string, PaletteReference> palettes = new Dictionary<string, PaletteReference>();

        readonly Func<string, PaletteReference> createPaletteReference;

        readonly HardwarePalette palette= new HardwarePalette();

        public event Action PaletteInvalidated = null;
        /// <summary>
        /// 开启深度缓冲z-Buffer
        /// </summary>
        readonly bool enableDepthBuffer;

        internal WorldRenderer(ModData mod,World world)
        {
            World = world;
            TileSize = World.Map.Grid.TileSize;
            TileScale = world.Map.Grid.Type == MapGridT.RectangularIsometric ? 1448 : 1024;
            ViewPort = new GameViewPort(this, world.Map);
            createPaletteReference = CreatePaletteReference;

            var mapGrid = mod.Manifest.Get<MapGrid>();
            enableDepthBuffer = mapGrid.EnableDepthBuffer;

            foreach (var pal in world.TraitDict.ActorsWithTrait<ILoadsPalettes>())
            {
                pal.Trait.LoadPalettes(this);
            }
            
            palette.Initialize();
            
            Theater = new Theater(world.Map.Rules.TileSet);

            terrainRenderer = new TerrainRenderer(world, this);
        }


        public void UpdatePalettesForPlayer(string internalName,HSLColor color,bool replaceExisting)
        {
        }

        /// <summary>
        /// 刷新调色板
        /// </summary>
        public void RefreshPalette()
        {
            palette.ApplyModifiers(World.WorldActor.TraitsImplementing<IPaletteModifier>());
            WarGame.Renderer.SetPalette(palette);
        }


        /// <summary>
        /// 
        /// </summary>
        public void Draw()
        {
            if (World.WorldActor.Disposed)
                return;

            RefreshPalette();

            var renderables = GenerateRenderables();
            var bounds = ViewPort.GetScissorBounds(World.Type != WorldT.Editor);
            
            WarGame.Renderer.EnableScissor(bounds);

            if (enableDepthBuffer)
                WarGame.Renderer.Device.EnableDepthBuffer();



            //地形绘制
            terrainRenderer.Draw(this, ViewPort);

            WarGame.Renderer.Flush();

            for (var i = 0; i < renderables.Count; i++)
                renderables[i].Render(this);


            foreach (var a in World.ActorsWithTrait<IRenderAboveWorld>())
                if (a.Actor.IsInWorld && !a.Actor.Disposed)
                    a.Trait.RenderAboveWorld(a.Actor, this);

            var renderShroud = World.RenderPlayer != null ? World.RenderPlayer.Shroud : null;

            if (enableDepthBuffer)
                WarGame.Renderer.ClearDepthBuffer();

            foreach (var a in World.ActorsWithTrait<IRenderShroud>())
                a.Trait.RenderShroud(renderShroud, this);

            if (enableDepthBuffer)
                WarGame.Renderer.DisableDepthBuffer();

            WarGame.Renderer.DisableScissor();

            var aboveShroud = World.ActorsWithTrait<IRenderAboveShroud>().Where(a => a.Actor.IsInWorld && !a.Actor.Disposed).SelectMany(a => a.Trait.RenderAboveShroud(a.Actor, this));

            var aboveShroudSelected = World.Selection.Actors.Where(a => !a.Disposed)
                                           .SelectMany(a => a.TraitsImplementing<IRenderAboveShroudWhenSelected>()
                                                       .SelectMany(t => t.RenderAboveShroud(a, this)));


            var aboveShroudEffects = World.Effects.Select(e => e as IEffectAboveShroud).Where(e => e != null).SelectMany(e => e.RenderAboveShroud(this));

            var aboveShroudOrderGenerator = SpriteRenderable.None;

            if (World.OrderGenerator != null)
                aboveShroudOrderGenerator = World.OrderGenerator.RenderAboveShroud(this, World);

            WarGame.Renderer.WorldModelRenderer.BeginFrame();

            var finalOverlayRenderables = aboveShroud.
                                                     Concat(aboveShroudSelected).
                                                     Concat(aboveShroudEffects).
                                                     Concat(aboveShroudOrderGenerator).
                                                     Select(r => r.PrepareRender(this)).ToList();

            WarGame.Renderer.WorldModelRenderer.EndFrame();

            ////HACK:Keep old grouping behaviour
            foreach (var g in finalOverlayRenderables.GroupBy(prs => prs.GetType()))
            {

                foreach (var r in g)
                {
                    r.Render(this);
                }
            }
            WarGame.Renderer.Flush();


        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        List<IFinalizedRenderable> GenerateRenderables()
        {
            var actors = World.ScreenMap.ActorsInBox(ViewPort.TopLeft, ViewPort.BottomRight).Append(World.WorldActor);

            if (World.RenderPlayer != null)
                actors = actors.Append(World.RenderPlayer.PlayerActor);

            var worldRenderables = actors.SelectMany(a => a.Render(this));

            if (World.OrderGenerator != null)
                worldRenderables = worldRenderables.Concat(World.OrderGenerator.Render(this, World));


            //Unpartitioned effects 
            //worldRenderables = worldRenderables.Concat(World.UnpartitionedEffects.SelectMany(a => a.Render(this)));
            worldRenderables = worldRenderables.Concat(World.Effects.SelectMany(e => e.Render(this)));

            //Partitioned, currently on-screen effects
            //var effectRenderables = World.ScreenMap.EffectsInBox(ViewPort.TopLeft, ViewPort.BottomRight);
            //worldRenderables = worldRenderables.Concat(effectRenderables, effectRenderables.SelectMany(e => e.Render(this));
            worldRenderables = worldRenderables.OrderBy(RenderableScreenZPositionComparisonKey);
            
            WarGame.Renderer.WorldModelRenderer.BeginFrame();
            var renderables = worldRenderables.Select(r => r.PrepareRender(this)).ToList();
            WarGame.Renderer.WorldModelRenderer.EndFrame();


            return renderables;
        }

        /// <summary>
        /// 缓存或内联调色代理，以避免重复分配
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public PaletteReference Palette(string name)
        {
            return palettes.GetOrAdd(name, createPaletteReference);
        }

        /// <summary>
        /// 将一个PaletteReference 缓存添加到World Renderer
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        PaletteReference CreatePaletteReference(string name)
        {
            var pal = palette.GetPalette(name);
            return new PaletteReference(name, palette.GetPaletteIndex(name), pal, palette);
        }

        /// <summary>
        /// 添加调色板
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pal"></param>
        /// <param name="allowModifiers"></param>
        /// <param name="allowOverwrite"></param>
        public void AddPalette(string name,ImmutablePalette pal,bool allowModifiers = false,bool allowOverwrite = false)
        {
            if(allowOverwrite && palette.Contains(name))
            {
                ReplacePalette(name, pal);
            }
            else
            {
                var oldHeight = palette.Height;
                palette.AddPalette(name, pal, allowModifiers);

                if (oldHeight != palette.Height && PaletteInvalidated != null)
                    PaletteInvalidated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pal"></param>
        public void ReplacePalette(string name,IPalette pal)
        {
            palette.ReplacePalette(name, pal);

            if (palettes.ContainsKey(name))
                palettes[name].Palette = pal;
        }

        /// <summary>
        /// Returns a position int the world that is projected to the given screen position.
        /// There are many possible world positions,and the returned value chooses the value with no elevation.
        /// </summary>
        /// <param name="screenPx"></param>
        /// <returns></returns>
        public WPos ProjectedPosition(Int2 screenPx)
        {
            return new WPos(TileScale * screenPx.X / TileSize.Width, TileScale * screenPx.Y / TileSize.Height, 0);
        }

        /// <summary>
        /// Conversion between world and screen coordinates
        /// 在世界坐标和屏幕坐标之间转换
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Vector2 ScreenPosition(WPos pos)
        {
            return new Vector2((float)TileSize.Width * pos.X / TileScale, (float)TileSize.Height * (pos.Y - pos.Z) / TileScale);
        }

        public Vector3 Screen3DPosition(WPos pos)
        {
            var z = ZPosition(pos, 0) *(float) TileSize.Height / TileScale;
            return new Vector3(TileSize.Width * pos.X / TileScale, TileSize.Height * (pos.Y - pos.Z) / TileScale, z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Int2 ScreenPxPosition(WPos pos)
        {
            //Round to nearest pixel(四舍五入到最近的象素点)
            var px = ScreenPosition(pos);
            return new Int2((int)Math.Round(px.X), (int)Math.Round(px.Y));
        }

        public Int2 ScreenPxOffset(WVec vec)
        {
            var xyz = ScreenVectorComponents(vec);
            return new Int2((int)Math.Round(xyz.X), (int)Math.Round(xyz.Y));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public Vector3 ScreenVectorComponents(WVec vec)
        {
            return new Vector3((float)TileSize.Width * vec.X / TileScale,
                                (float)TileSize.Height * (vec.Y - vec.Z) / TileScale,
                                (float)TileSize.Height * vec.Z / TileScale);
        }

        /// <summary>
        /// For scaling vectors to pixel sizes in the model renderer
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public float[] ScreenVector(WVec vec)
        {
            var xyz = ScreenVectorComponents(vec);
            return new[] { xyz.X, xyz.Y, xyz.Z, 1f };
        }

        public float ScreenZPosition(WPos pos,int offset)
        {
            return ZPosition(pos, offset) * (float)TileSize.Height / TileScale;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        static int ZPosition(WPos pos,int offset)
        {
            return pos.Y + pos.Z + offset;
        }


        /// <summary>
        /// Dispose the specified disposing.
        /// </summary>
        /// <returns>The dispose.</returns>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>

        public void Dispose()
        {
            //HACK: Disposing the world from here violates ownership 
            //but the WorldRenderer lifetime matches the disposal 
            //behavior we want for the world.and the root object setup is horrible that doing it properly would be a giant mess.
            World.Dispose();
            palette.Dispose();
            Theater.Dispose();
            terrainRenderer.Dispose();
        }
    }
}
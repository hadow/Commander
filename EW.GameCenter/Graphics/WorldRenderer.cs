using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EW.Xna.Platforms;
using EW.Traits;
using EW.Xna.Platforms.Graphics;
namespace EW.Graphics
{
    /// <summary>
    /// 世界渲染器
    /// </summary>
    public sealed class WorldRenderer:DrawableGameComponent
    {
        public static readonly Func<IRenderable, int> RenderableScreenZPositionComparisonKey = r => ZPosition(r.Pos, r.ZOffset);

        public GameViewPort ViewPort { get; private set; }

        public readonly Size TileSize;

        public readonly World World;

        public readonly Theater Theater;

        readonly TerrainRenderer terrainRenderer;

        readonly Dictionary<string, PaletteReference> palettes = new Dictionary<string, PaletteReference>();

        readonly Func<string, PaletteReference> createPaletteReference;

        readonly HardwarePalette palette; //= new HardwarePalette();

        public event Action PaletteInvalidated = null;
        /// <summary>
        /// 开启深度缓冲z-Buffer
        /// </summary>
        readonly bool enableDepthBuffer;

        internal WorldRenderer(Game game,ModData mod,World world):base(game)
        {
            World = world;
            TileSize = World.Map.Grid.TileSize;
            ViewPort = new GameViewPort(game,this, world.Map);
            palette = new HardwarePalette(game);
            createPaletteReference = CreatePaletteReference;

            var mapGrid = mod.Manifest.Get<MapGrid>();
            enableDepthBuffer = mapGrid.EnableDepthBuffer;

            foreach (var pal in world.TraitDict.ActorsWithTrait<ILoadsPalettes>())
            {
                pal.Trait.LoadPalettes(this);
            }

            palette.Initialize();
            
            Theater = new Theater(game,world.Map.Rules.TileSet);

            terrainRenderer = new TerrainRenderer(game,world, this);
        }

        /// <summary>
        /// 刷新调色板
        /// </summary>
        public void RefreshPalette()
        {
            palette.ApplyModifiers(World.WorldActor.TraitsImplementing<IPaletteModifier>());
            ((WarGame)Game).Renderer.SetPalette(palette);
        }


        /// <summary>
        /// 
        /// </summary>
        public void Draw()
        {
            if (World.WorldActor.Disposed)
                return;

            RefreshPalette();


            if (enableDepthBuffer)
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            //var renderables = GenerateRenderables();

            var bounds = ViewPort.GetScissorBounds(World.Type != WorldT.Editor);
            terrainRenderer.Draw(this, ViewPort);

            if (enableDepthBuffer)
                GraphicsDevice.DepthStencilState = DepthStencilState.None;
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

            worldRenderables = worldRenderables.OrderBy(RenderableScreenZPositionComparisonKey);
            ((WarGame)this.Game).Renderer.WorldVoxelRenderer.BeginFrame();

            return null;
        }

        /// <summary>
        /// 缓存或内联一些代理，以避免重复分配
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
        public WPos ProjectedPosition(EW.Xna.Platforms.Point screenPx)
        {
            return new WPos(1024 * screenPx.X / TileSize.Width, 1024 * screenPx.Y / TileSize.Height, 0);
        }

        /// <summary>
        /// Conversion between world and screen coordinates
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Vector2 ScreenPosition(WPos pos)
        {
            return new Vector2(TileSize.Width * pos.X / 1024f, TileSize.Height * (pos.Y - pos.Z) / 1024f);
        }

        public Vector3 Screen3DPosition(WPos pos)
        {
            var z = ZPosition(pos, 0) * TileSize.Height / 1024f;
            return new Vector3(TileSize.Width * pos.X / 1024f, TileSize.Height * (pos.Y - pos.Z) / 1024f, z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public EW.Xna.Platforms.Point ScreenPxPosition(WPos pos)
        {
            //Round to nearest pixel(四舍五入到最近的象素点)
            var px = ScreenPosition(pos);
            return new Xna.Platforms.Point((int)Math.Round(px.X), (int)Math.Round(px.Y));
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


        public void Dispose()
        {
            World.Dispose();

            Theater.Dispose();
            terrainRenderer.Dispose();
        }
    }
}
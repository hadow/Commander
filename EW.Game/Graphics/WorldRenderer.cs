using System;
using System.Collections.Generic;
using System.Drawing;
using EW.Xna.Platforms;
namespace EW.Graphics
{
    /// <summary>
    /// 世界渲染器
    /// </summary>
    public sealed class WorldRenderer:IDisposable
    {

        public GameViewPort ViewPort { get; private set; }

        public readonly Size TileSize;

        public readonly World World;

        public readonly Theater Theater;

        readonly TerrainRenderer terrainRenderer;

        readonly Dictionary<string, PaletteReference> palettes = new Dictionary<string, PaletteReference>();

        readonly Func<string, PaletteReference> createPaletteReference;

        readonly HardwarePalette palette = new HardwarePalette();

        public event Action PaletteInvalidated = null;
        /// <summary>
        /// 开启深度缓冲z-Buffer
        /// </summary>
        readonly bool enableDepthBuffer;

        internal WorldRenderer(ModData mod,World world)
        {
            World = world;
            TileSize = World.Map.Grid.TileSize;
            ViewPort = new GameViewPort(this, world.Map);

            createPaletteReference = CreatePaletteReference;

            foreach (var pal in world.TraitDict.ActorsWithTrait<ILoadsPalettes>())
            {
                pal.Trait.LoadPalettes(this);
            }

            palette.Initialize();

            
            var mapGrid = mod.Manifest.Get<MapGrid>();
            enableDepthBuffer = mapGrid.EnableDepthBuffer;

            Theater = new Theater(world.Map.Rules.TileSet);

            terrainRenderer = new TerrainRenderer(world, this);
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
            terrainRenderer.Draw(this, ViewPort);
        }

        public PaletteReference Palette(string name)
        {
            return palettes.GetOrAdd(name, createPaletteReference);
        }

        /// <summary>
        /// 
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
        /// 
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
            return new Vector3(TileSize.Width * pos.X / 1024f, TileSize.Height * (pos.Y - pos.Z) / 1024f, 0);
        }

        public EW.Xna.Platforms.Point ScreenPxPosition(WPos pos)
        {
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
using System;
using System.Drawing;
using EW.Graphics;
using EW.Xna.Platforms.Graphics;
using EW.Xna.Platforms;
using EW.Xna.Platforms.Content;
namespace EW
{
    public sealed class Renderer:DrawableGameComponent
    {
        public SpriteRenderer SpriteRenderer;

        public SpriteRenderer WorldSpriteRenderer;

        public SpriteRenderer RgbaSpriteRenderer;

        public SpriteRenderer WorldRgbaSpriteRenderer;

        public RgbaColorRenderer RgbaColorRenderer;

        public RgbaColorRenderer WorldRgbaColorRenderer;


        internal int TempBufferSize { get; private set; }
        
        internal int SheetSize { get; private set; }
        Size? lastResolution;
        EW.Xna.Platforms.Point? lastScroll;
        float? lastZoom;

        Texture2D currentPaletteTexture;
        
        public Renderer(Game game,GraphicsSettings graphicSettings):base(game)
        {

            TempBufferSize = graphicSettings.BatchSize;
            SheetSize = graphicSettings.SheetSize;
           
            SpriteRenderer = new SpriteRenderer(this, this.Game.Content.Load<Effect>("shp"));
        }
        public void BeginFrame(EW.Xna.Platforms.Point scroll,float zoom)
        {
            GraphicsDeviceManager.M.GraphicsDevice.Clear(EW.Xna.Platforms.Color.White);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="scroll"></param>
        /// <param name="zoom"></param>
        public void SetViewportParams(EW.Xna.Platforms.Point scroll,float zoom)
        {

        }

        public void SetPalette(HardwarePalette palette)
        {
            if (palette.Texture == currentPaletteTexture)
                return;

            currentPaletteTexture = palette.Texture;


        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="numVertices"></param>
        /// <param name="type"></param>
        public void DrawBatch(Vertex[] vertices,int numVertices,PrimitiveType type)
        {

        }
    }
}
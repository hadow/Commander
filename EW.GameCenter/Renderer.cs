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

        float depthScale, depthOffset;

        internal int SheetSize { get; private set; }
        Size? lastResolution;
        EW.Xna.Platforms.Point? lastScroll;
        float? lastZoom;

        Texture2D currentPaletteTexture;
        

        public Size Resolution { get { return new Size(GraphicsDevice.DisplayMode.Width,GraphicsDevice.DisplayMode.Height); } }
        public Renderer(Game game,GraphicsSettings graphicSettings):base(game)
        {

            TempBufferSize = graphicSettings.BatchSize;
            SheetSize = graphicSettings.SheetSize;
           
            SpriteRenderer = new SpriteRenderer(this, this.Game.Content.Load<Effect>("Content/GLSL/SpriteEffect.ogl"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapGrid"></param>
        public void InitializeDepthBuffer(MapGrid mapGrid)
        {

        }

        public void BeginFrame(EW.Xna.Platforms.Point scroll,float zoom)
        {
            GraphicsDevice.Clear(EW.Xna.Platforms.Color.White);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="scroll"></param>
        /// <param name="zoom"></param>
        public void SetViewportParams(EW.Xna.Platforms.Point scroll,float zoom)
        {
            if(lastScroll != scroll || lastZoom != zoom)
            {
                lastScroll = scroll;
                lastZoom = zoom;

                WorldSpriteRenderer.SetViewportParams(Resolution, depthScale, depthOffset, zoom, scroll);
            }
        }

        public void SetPalette(HardwarePalette palette)
        {
            if (palette.Texture == currentPaletteTexture)
                return;

            currentPaletteTexture = palette.Texture;

            WorldSpriteRenderer.SetPalette(currentPaletteTexture);

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
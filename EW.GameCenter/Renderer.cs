using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EW.Graphics;
using EW.Xna.Platforms.Graphics;
using EW.Xna.Platforms;
using EW.Xna.Platforms.Content;
namespace EW
{
    /// <summary>
    /// 
    /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        readonly Stack<Xna.Platforms.Rectangle> scissorState = new Stack<Xna.Platforms.Rectangle>();

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

        public void EndFrame()
        {

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


        public void EnableScissor(EW.Xna.Platforms.Rectangle rect)
        {
            this.GraphicsDevice.ScissorRectangle = rect;
            scissorState.Push(rect);
            
        }

        public void DisableScissor()
        {
            scissorState.Pop();

            if (scissorState.Any())
            {
                var rect = scissorState.Peek();
            }
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
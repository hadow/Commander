using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EW.Graphics;
using EW.Xna.Platforms.Graphics;
using EW.Xna.Platforms;
namespace EW
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Renderer:DrawableGameComponent
    {

        public interface IBatchRenderer { void Flush(); }

        public IBatchRenderer CurrentBatchRenderer
        {
            get { return currentBatchRenderer; }
            set {
                if (currentBatchRenderer == value)
                    return;
                if(currentBatchRenderer != null)
                    currentBatchRenderer.Flush();

                currentBatchRenderer = value;
            }
        }

        public SpriteRenderer SpriteRenderer;

        public SpriteRenderer WorldSpriteRenderer;

        public SpriteRenderer RgbaSpriteRenderer;

        public SpriteRenderer WorldRgbaSpriteRenderer;

        public RgbaColorRenderer RgbaColorRenderer;

        public RgbaColorRenderer WorldRgbaColorRenderer;


        public ModelRenderer WorldModelRenderer { get; private set; }

        internal int TempBufferSize { get; private set; }

        readonly VertexBuffer tempBuffer;
        readonly VertexBufferBinding[] bindings;
        float depthScale, depthOffset;

        internal int SheetSize { get; private set; }
        Size? lastResolution;
        Int2? lastScroll;
        float? lastZoom;

        Texture2D currentPaletteTexture;
        IBatchRenderer currentBatchRenderer;
        /// <summary>
        /// 
        /// </summary>
        readonly Stack<Xna.Platforms.Rectangle> scissorState = new Stack<Xna.Platforms.Rectangle>();

        public Size Resolution { get { return new Size(GraphicsDevice.DisplayMode.Width,GraphicsDevice.DisplayMode.Height); } }
        public Renderer(Game game,GraphicsSettings graphicSettings):base(game)
        {

            TempBufferSize = graphicSettings.BatchSize;
            SheetSize = graphicSettings.SheetSize;

            WorldSpriteRenderer = new SpriteRenderer(this, this.Game.Content.Load<Effect>("Content/glsl/shp"));
            SpriteRenderer = new SpriteRenderer(this, this.Game.Content.Load<Effect>("Content/glsl/shp"));
            //WorldModelRenderer = new ModelRenderer(this, this.Game.Content.Load<Effect>("Content/glsl/model"));

            tempBuffer = new VertexBuffer(GraphicsDevice, typeof(Vertex), TempBufferSize, BufferUsage.None);
            this.bindings = new VertexBufferBinding[1];
            this.bindings[0] = new VertexBufferBinding(tempBuffer) ;
        }

        /// <summary>
        /// 初始化深度缓冲区
        /// </summary>
        /// <param name="mapGrid"></param>
        public void InitializeDepthBuffer(MapGrid mapGrid)
        {
            this.depthScale = mapGrid == null || !mapGrid.EnableDepthBuffer ? 0 : (float)Resolution.Height / (Resolution.Height + mapGrid.TileSize.Height * mapGrid.MaximumTerrainHeight);
            this.depthOffset = this.depthScale / 2;
        }

        public void BeginFrame(Int2 scroll,float zoom)
        {
            GraphicsDevice.Clear(EW.Xna.Platforms.Color.White);
            SetViewportParams(scroll, zoom);
        }

        public void EndFrame()
        {
            Flush();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="scroll"></param>
        /// <param name="zoom"></param>
        public void SetViewportParams(Int2 scroll,float zoom)
        {

            var resolutionChanged = lastResolution != Resolution;

            if (resolutionChanged)
            {

            }

            //If zoom evaluates as different due to floating point weirdness that's ok,setting the parameters again is harmless.
            if(resolutionChanged || lastScroll != scroll || lastZoom != zoom)
            {
                lastScroll = scroll;
                lastZoom = zoom;

                WorldSpriteRenderer.SetViewportParams(Resolution, depthScale, depthOffset, zoom, scroll);
                //WorldModelRenderer.SetViewportParams(Resolution, zoom, scroll);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="palette"></param>
        public void SetPalette(HardwarePalette palette)
        {
            if (palette.Texture == currentPaletteTexture)
                return;

            currentPaletteTexture = palette.Texture;

            WorldSpriteRenderer.SetPalette(currentPaletteTexture);
            //WorldModelRenderer.SetPalette(currentPaletteTexture);
        }

        public void Flush()
        {
            CurrentBatchRenderer = null;
        }


        public void EnableScissor(EW.Xna.Platforms.Rectangle rect)
        {
            //Must remain inside the current scissor rect.
            if (scissorState.Any())
                rect.Intersects(scissorState.Peek());
            Flush();
            this.GraphicsDevice.ScissorRectangle = rect;
            scissorState.Push(rect);
            
        }

        public void DisableScissor()
        {
            scissorState.Pop();
            Flush();

            //Restore previous scissor rect
            if (scissorState.Any())
            {
                var rect = scissorState.Peek();
                this.GraphicsDevice.ScissorRectangle = rect;
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
            tempBuffer.SetData(vertices);
            DrawBatch(tempBuffer, 0, numVertices, type);
        }

        public void DrawBatch(VertexBuffer vertices, int firstVertext, int numVertices, PrimitiveType type) {

            GraphicsDevice.DrawPrimitives(type, firstVertext, numVertices);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            //WorldModelRenderer.Dispose();
        }
    }
}
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EW.Graphics;
using EW.Framework;
using EW.Framework.Graphics;
namespace EW
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Renderer
    {

        public interface IBatchRenderer { void Flush(); }
        ITexture currentPaletteTexture;
        IBatchRenderer currentBatchRenderer;
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

        internal IGraphicsDevice Device { get; private set; }

        public SpriteRenderer SpriteRenderer;

        public SpriteRenderer WorldSpriteRenderer;

        public SpriteRenderer RgbaSpriteRenderer;

        public SpriteRenderer WorldRgbaSpriteRenderer;

        public RgbaColorRenderer RgbaColorRenderer;

        public RgbaColorRenderer WorldRgbaColorRenderer;


        public ModelRenderer WorldModelRenderer { get; private set; }

        internal int TempBufferSize { get; private set; }

        readonly IVertexBuffer<Vertex> tempBuffer;
        float depthScale, depthOffset;

        internal int SheetSize { get; private set; }
        Size? lastResolution;
        Int2? lastScroll;
        float? lastZoom;

        
        /// <summary>
        /// 
        /// </summary>
        readonly Stack<EW.Framework.Rectangle> scissorState = new Stack<EW.Framework.Rectangle>();

        public Size Resolution { get { return new Size(Device.Viewport.Width,Device.Viewport.Height); } }
        public Renderer(GraphicsSettings graphicSettings,IGraphicsDevice device)
        {
            this.Device = device;
            TempBufferSize = graphicSettings.BatchSize;
            SheetSize = graphicSettings.SheetSize;

            WorldModelRenderer = new ModelRenderer(this, Device.CreateShader("model"));
            WorldSpriteRenderer = new SpriteRenderer(this, Device.CreateShader("shp"));
            WorldRgbaColorRenderer = new RgbaColorRenderer(this, Device.CreateShader("color"));
            WorldRgbaSpriteRenderer = new SpriteRenderer(this, Device.CreateShader("rgba"));
            SpriteRenderer = new SpriteRenderer(this, Device.CreateShader("shp"));
            RgbaColorRenderer = new RgbaColorRenderer(this, Device.CreateShader("color"));
            RgbaSpriteRenderer = new SpriteRenderer(this, Device.CreateShader("rgba"));
            tempBuffer = Device.CreateVertexBuffer(TempBufferSize);
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
            Device.Clear();
            SetViewportParams(scroll, zoom);
        }

        public void EndFrame()
        {
            Flush();
            Device.Present();
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
                lastResolution = Resolution;
                RgbaSpriteRenderer.SetViewportParams(Resolution, 0f, 0f, 1f, Int2.Zero);
                SpriteRenderer.SetViewportParams(Resolution, 0f, 0f, 1f, Int2.Zero);
                RgbaColorRenderer.SetViewportParams(Resolution, 0f, 0f, 1f, Int2.Zero);
            }

            //If zoom evaluates as different due to floating point weirdness that's ok,setting the parameters again is harmless.
            if(resolutionChanged || lastScroll != scroll || lastZoom != zoom)
            {
                lastScroll = scroll;
                lastZoom = zoom;

                WorldRgbaSpriteRenderer.SetViewportParams(Resolution, depthScale, depthOffset, zoom, scroll);
                WorldSpriteRenderer.SetViewportParams(Resolution, depthScale, depthOffset, zoom, scroll);
                WorldModelRenderer.SetViewportParams(Resolution, zoom, scroll);
                WorldRgbaColorRenderer.SetViewportParams(Resolution, depthScale, depthOffset, zoom, scroll);
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

            Flush();
            currentPaletteTexture = palette.Texture;

            RgbaSpriteRenderer.SetPalette(currentPaletteTexture);
            SpriteRenderer.SetPalette(currentPaletteTexture);
            WorldSpriteRenderer.SetPalette(currentPaletteTexture);
            WorldRgbaSpriteRenderer.SetPalette(currentPaletteTexture);
            WorldModelRenderer.SetPalette(currentPaletteTexture);
        }

        public void Flush()
        {
            CurrentBatchRenderer = null;
        }


        public void EnableScissor(EW.Framework.Rectangle rect)
        {
            //Must remain inside the current scissor rect.
            if (scissorState.Any())
                rect.Intersects(scissorState.Peek());
            Flush();
            Device.EnableScissor(rect.Left, rect.Top, rect.Width, rect.Height);
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
                Device.EnableScissor(rect.Left, rect.Top, rect.Width, rect.Height);
            }
            else
            {
                Device.DisableScissor();
            }
        }

        public void EnableDepthBuffer()
        {

        }


        public void DisableDepthBuffer()
        {
            Flush();
            Device.DisableDepthBuffer();
        }


        public void ClearDepthBuffer()
        {
            Flush();
            Device.ClearDepthBuffer();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="numVertices"></param>
        /// <param name="type"></param>
        public void DrawBatch(Vertex[] vertices,int numVertices,PrimitiveType type)
        {
            tempBuffer.SetData(vertices,numVertices);
            DrawBatch(tempBuffer, 0, numVertices, type);
        }
        

        public void DrawBatch<T>(IVertexBuffer<T> vertices,int firstVertex,int numVertices,PrimitiveType type) where T : struct
        {
            vertices.Bind();
            Device.DrawPrimitives(type, firstVertex, numVertices);

        }


        public IVertexBuffer<Vertex> CreateVertexBuffer(int length)
        {
            return Device.CreateVertexBuffer(length);
        }


        public void Dispose()
        {
            Device.Dispose();
            WorldModelRenderer.Dispose();
            tempBuffer.Dispose();

        }
    }
}
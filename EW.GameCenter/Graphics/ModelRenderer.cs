using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using EW.Xna.Platforms;
using EW.Primitives;
using EW.Xna.Platforms.Graphics;
namespace EW.Graphics
{
    public class ModelRenderProxy
    {
        public readonly Sprite Sprite;
        public readonly Sprite ShadowSprite;
        public readonly float ShadowDirection;
        public readonly Vector3[] ProjectedShadowBounds;

        public ModelRenderProxy(Sprite sprite,Sprite shadowSprite,Vector3[] projectedShadowBounds,float shadowDirection)
        {
            Sprite = sprite;
            ShadowSprite = shadowSprite;
            ProjectedShadowBounds = projectedShadowBounds;
            ShadowDirection = shadowDirection;
        }
    }

    /// <summary>
    /// 模型渲染
    /// </summary>
    public sealed class ModelRenderer:IDisposable
    {
        static readonly float[] ShadowDiffuse = new float[] { 0, 0, 0 };
        static readonly float[] ShadowAmbient = new float[] { 1, 1, 1 };

        static readonly Vector2 SpritePadding = new Vector2(2, 2);

        readonly List<Pair<Sheet, Action>> doRender = new List<Pair<Sheet, Action>>();

        readonly Renderer renderer;

        readonly Effect shader;

        readonly Dictionary<Sheet, RenderTarget2D> mappedBuffers = new Dictionary<Sheet, RenderTarget2D>();

        readonly Stack<KeyValuePair<Sheet, RenderTarget2D>> unmappedBuffers = new Stack<KeyValuePair<Sheet, RenderTarget2D>>();

        SheetBuilder sheetBuilder;

        public ModelRenderer(Renderer renderer,Effect shader)
        {
            this.renderer = renderer;
            this.shader = shader;
        }

        public void SetPalette(Texture palette)
        {
            shader.Parameters["Palette"].SetValue(palette);
        }

        public void SetViewportParams(Size screen,float zoom,Int2 scroll)
        {
            var a = 2f / renderer.SheetSize;
            var view = new Matrix(
                a,0,0,0,
                0,-a,0,0,
                0,0,-2*a,0,
                -1,1,0,1

                );
            shader.Parameters["View"].SetValue(view);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderData"></param>
        /// <param name="t"></param>
        /// <param name="lightDirection"></param>
        /// <param name="ambientLight"></param>
        /// <param name="diffuseLight"></param>
        /// <param name="colorPaletteTextureMidIndex"></param>
        /// <param name="normalsPaletteTextureMidIndex"></param>
        void Render(ModelRenderData renderData,float[] t,float[] lightDirection,
            float[] ambientLight,float[] diffuseLight,float colorPaletteTextureMidIndex,float normalsPaletteTextureMidIndex)
        {
            shader.Parameters["DiffuseTexture"].SetValue(renderData.Sheet.GetTexture());
            shader.Parameters["PaletteRows"].SetValue(new Vector2(colorPaletteTextureMidIndex, normalsPaletteTextureMidIndex));
            //shader.Parameters["TransformMatrix"].SetValue(new Matrix());
            //shader.Parameters["LightDirection"].set
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="wr"></param>
        /// <param name="models"></param>
        /// <param name="camera"></param>
        /// <param name="scale"></param>
        /// <param name="groundNormal"></param>
        /// <param name="lightSource"></param>
        /// <param name="lightAmbientColor"></param>
        /// <param name="lightDiffuseColor"></param>
        /// <param name="color"></param>
        /// <param name="normals"></param>
        /// <param name="shadowPalette"></param>
        /// <returns></returns>
        public ModelRenderProxy RenderAsync(WorldRenderer wr,IEnumerable<ModelAnimation> models,WRot camera,float scale,
            float[] groundNormal,WRot lightSource,float[] lightAmbientColor,float[] lightDiffuseColor,PaletteReference color,PaletteReference normals,PaletteReference shadowPalette)
        {
            //Sprite rectangle
            var tl = new Vector2(float.MaxValue, float.MaxValue);
            var br = new Vector2(float.MinValue, float.MinValue);

            //Shadow sprite rectangle

            var stl = new Vector2(float.MaxValue, float.MaxValue);
            var sbr = new Vector2(float.MinValue, float.MinValue);


            var screenCorners = new Vector3[4];


            Size spriteSize, shadowSpriteSize;
            Int2 spriteOffset, shadowSpriteOffset;

            CalculateSpriteGeometry(tl, br, 1, out spriteSize, out spriteOffset);
            CalculateSpriteGeometry(stl, sbr, 2, out shadowSpriteSize, out shadowSpriteOffset);

            var sprite = sheetBuilder.Allocate(spriteSize, 0, spriteOffset);
            var shadowSprite = sheetBuilder.Allocate(shadowSpriteSize, 0, shadowSpriteOffset);
            return new ModelRenderProxy(sprite,shadowSprite,screenCorners,0);
        }

        /// <summary>
        /// 计算精灵的几何形体
        /// </summary>
        /// <param name="tl"></param>
        /// <param name="br"></param>
        /// <param name="scale"></param>
        /// <param name="size"></param>
        /// <param name="offset"></param>
        static void CalculateSpriteGeometry(Vector2 tl,Vector2 br,float scale,out Size size,out Int2 offset)
        {
            var width = (int)(scale * (br.X - tl.X));
            var height = (int)(scale * (br.Y - tl.Y));
            offset = (0.5f * scale * (br + tl)).ToInt2();

            // width and height must be even to avoid rendering glitches
            if ((width & 1) == 1)
                width += 1;
            if ((height & 1) == 1)
                height += 1;
            size = new Size(width, height);


        }


        public void BeginFrame()
        {
            foreach (var kv in mappedBuffers)
                unmappedBuffers.Push(kv);
            mappedBuffers.Clear();

            sheetBuilder = new SheetBuilder(this.renderer.Game,SheetT.BGRA, AllocateSheet);
            doRender.Clear();    
        }


        public void EndFrame()
        {
            if (doRender.Count == 0)
                return;

            Sheet currentSheet = null;
            
            foreach(var v in doRender)
            {
                //Change sheet
                if(v.First != currentSheet)
                {
                    currentSheet = v.First;

                }

                v.Second();
            }
        }

        RenderTarget2D EnableFrameBuffer(Sheet s)
        {
            var fbo = mappedBuffers[s];

            return fbo;
        }

        public Sheet AllocateSheet()
        {
            //Reuse cached fbo;
            if (unmappedBuffers.Count > 0)
            {
                var kv = unmappedBuffers.Pop();
                mappedBuffers.Add(kv.Key, kv.Value);
                return kv.Key;
                    
            }

            var size = new Size(renderer.SheetSize, renderer.SheetSize);
            var frameBuffer = new RenderTarget2D(this.renderer.GraphicsDevice, size.Width, size.Height);
            var sheet = new Sheet(this.renderer.Game, SheetT.BGRA, frameBuffer);
            return sheet;
        }


        public void Dispose()
        {
            foreach(var kvp in mappedBuffers.Concat(unmappedBuffers))
            {
                kvp.Key.Dispose();
                kvp.Value.Dispose();
            }


            mappedBuffers.Clear();
            unmappedBuffers.Clear();
        }





    }
}
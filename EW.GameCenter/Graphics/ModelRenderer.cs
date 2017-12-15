using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;

using EW.Primitives;
using EW.OpenGLES;
using EW.OpenGLES.Graphics;
namespace EW.Graphics
{
    public class ModelRenderProxy
    {
        public readonly Sprite Sprite;
        public readonly Sprite ShadowSprite;
        public readonly float ShadowDirection;  //影子朝向
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
        //Static constants
        static readonly float[] ShadowDiffuse = new float[] { 0, 0, 0 };
        static readonly float[] ShadowAmbient = new float[] { 1, 1, 1 };

        static readonly Vector2 SpritePadding = new Vector2(2, 2);

        static readonly float[] ZeroVector = new float[] { 0, 0, 0, 1 };
        static readonly float[] ZVector = new float[] { 0, 0, 1, 1 };
        static readonly float[] FlipMtx = Util.ScaleMaxtrix(1, -1, 1);
        static readonly float[] ShadowScaleFlipMtx = Util.ScaleMaxtrix(2, -2, 2);

        readonly List<Pair<Sheet, Action>> doRender = new List<Pair<Sheet, Action>>();

        readonly Renderer renderer;

        readonly IShader shader;

        readonly Dictionary<Sheet, IFrameBuffer> mappedBuffers = new Dictionary<Sheet, IFrameBuffer>();

        readonly Stack<KeyValuePair<Sheet, IFrameBuffer>> unmappedBuffers = new Stack<KeyValuePair<Sheet, IFrameBuffer>>();

        SheetBuilder sheetBuilder;

        public ModelRenderer(Renderer renderer,IShader shader)
        {
            this.renderer = renderer;
            this.shader = shader;
        }

        public void SetPalette(ITexture palette)
        {
            //shader.Parameters["Palette"].SetValue(palette);
            shader.SetTexture("Palette", palette);
        }

        public void SetViewportParams(Size screen, float zoom, Int2 scroll)
        {
            var a = 2f / renderer.SheetSize;
            var view = new float[]{
                a, 0, 0, 0,
                0, -a, 0, 0,
                0, 0, -2 * a, 0,
                -1, 1, 0, 1

                };
            //shader.Parameters["View"].SetValue(view);
            shader.SetMatrix("View", view);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderData"></param>
        /// <param name="cache"></param>
        /// <param name="t"></param>
        /// <param name="lightDirection"></param>
        /// <param name="ambientLight"></param>
        /// <param name="diffuseLight"></param>
        /// <param name="colorPaletteTextureMidIndex"></param>
        /// <param name="normalsPaletteTextureMidIndex"></param>
        void Render(ModelRenderData renderData,IModelCache cache,float[] t,float[] lightDirection,
            float[] ambientLight,float[] diffuseLight,float colorPaletteTextureMidIndex,float normalsPaletteTextureMidIndex)
        {
            //shader.Parameters["DiffuseTexture"].SetValue(renderData.Sheet.GetTexture());
            //shader.Parameters["PaletteRows"].SetValue(new Vector2(colorPaletteTextureMidIndex, normalsPaletteTextureMidIndex));
            //shader.Parameters["TransformMatrix"].SetValue(new Matrix(t));
            //shader.Parameters["LightDirection"].SetValue(new Vector4(lightDirection[0], lightDirection[1], lightDirection[2], lightDirection[3]));
            //shader.Parameters["AmbientLight"].SetValue(new Vector3(ambientLight[0], ambientLight[1], ambientLight[2]));
            //shader.Parameters["DiffuseLight"].SetValue(new Vector3(diffuseLight[0], diffuseLight[1], diffuseLight[2]));
            
            
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="wr"></param>
        /// <param name="models"></param>
        /// <param name="camera"></param>
        /// <param name="scale">缩放</param>
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

            //Correct for inverted y-axis
            var scaleTransform = Util.ScaleMaxtrix(scale, scale, scale);

            //Correct for bogus light source definition
            var lightYaw = Util.MakeFloatMatrix(new WRot(WAngle.Zero, WAngle.Zero, -lightSource.Yaw).AsMatrix());
            var lightPitch = Util.MakeFloatMatrix(new WRot(WAngle.Zero, -lightSource.Pitch, WAngle.Zero).AsMatrix());
            var shadowTransform = Util.MatrixMultiply(lightPitch, lightYaw);

            var invShadowTransform = Util.MatrixInverse(shadowTransform);
            var cameraTransform = Util.MakeFloatMatrix(camera.AsMatrix());
            var invCameraTransform = Util.MatrixInverse(cameraTransform);
            if (invCameraTransform == null)
                throw new InvalidOperationException("Failed to invert the cameraTransform matrix during RenderAsync");


            //Sprite rectangle
            var tl = new Vector2(float.MaxValue, float.MaxValue);
            var br = new Vector2(float.MinValue, float.MinValue);

            //Shadow sprite rectangle

            var stl = new Vector2(float.MaxValue, float.MaxValue);
            var sbr = new Vector2(float.MinValue, float.MinValue);

            foreach (var m in models)
            {
                // Convert screen offset back to world coords
                var offsetVec = Util.MatrixVectorMultiply(invCameraTransform, wr.ScreenVector(m.OffsetFunc()));
                var offsetTransform = Util.TranslationMatrix(offsetVec[0], offsetVec[1], offsetVec[2]);

                var worldTransform = m.RotationFunc().Aggregate(Util.IdentityMatrix(), (x, y) => Util.MatrixMultiply(Util.MakeFloatMatrix(y.AsMatrix()), x));
                worldTransform = Util.MatrixMultiply(scaleTransform, worldTransform);
                worldTransform = Util.MatrixMultiply(offsetTransform, worldTransform);

                var bounds = m.Model.Bounds(m.FrameFunc());
                var worldBounds = Util.MatrixAABBMultiply(worldTransform, bounds);
                var screenBounds = Util.MatrixAABBMultiply(cameraTransform, worldBounds);
                var shadowBounds = Util.MatrixAABBMultiply(shadowTransform, worldBounds);

                //Aggregate bounds rects
                tl = Vector2.Min(tl, new Vector2(screenBounds[0], screenBounds[1]));
                br = Vector2.Max(br, new Vector2(screenBounds[3], screenBounds[4]));
                stl = Vector2.Min(stl, new Vector2(shadowBounds[0], shadowBounds[1]));
                sbr = Vector2.Max(sbr, new Vector2(shadowBounds[3], shadowBounds[4]));
            }

            //Inflate rects to ensure redering is within bounds
            tl -= SpritePadding;
            br += SpritePadding;
            stl -= SpritePadding;
            sbr += SpritePadding;

            //Corners of the shadow quad,in shadow-space
            var corners = new float[][]
            {
                new[]{stl.X,stl.Y,0,1},
                new[]{sbr.X,sbr.Y,0,1},
                new[]{sbr.X,sbr.Y,0,1},
                new[]{stl.X,stl.Y,0,1}
            };

            var shadowScreenTransform = Util.MatrixMultiply(cameraTransform, invShadowTransform);
            var shadowGroundNormal = Util.MatrixVectorMultiply(shadowTransform, groundNormal);
            var screenCorners = new Vector3[4];

            for(var j = 0; j < 4; j++)
            {
                //Project to ground plane.
                corners[j][2] = -(corners[j][1] * shadowGroundNormal[1] / shadowGroundNormal[2] +
                                corners[j][0] * shadowGroundNormal[0] / shadowGroundNormal[2]);

                //Rotate to camera-space
                corners[j] = Util.MatrixVectorMultiply(shadowScreenTransform, corners[j]);
                screenCorners[j] = new Vector3(corners[j][0], corners[j][1], 0);


            }

            //Shadows are rendered at twice the resolution to reduce artifacts.
            //阴影以两倍的分辨率渲染以减少伪像
            Size spriteSize, shadowSpriteSize;
            Int2 spriteOffset, shadowSpriteOffset;

            CalculateSpriteGeometry(tl, br, 1, out spriteSize, out spriteOffset);
            CalculateSpriteGeometry(stl, sbr, 2, out shadowSpriteSize, out shadowSpriteOffset);

            var sprite = sheetBuilder.Allocate(spriteSize, 0, spriteOffset);
            var shadowSprite = sheetBuilder.Allocate(shadowSpriteSize, 0, shadowSpriteOffset);

            var sb = sprite.Bounds;
            var ssb = shadowSprite.Bounds;

            var spriteCenter = new Vector2(sb.Left + sb.Width / 2, sb.Top + sb.Height / 2);
            var shadowCenter = new Vector2(ssb.Left + ssb.Width / 2, ssb.Top + ssb.Height / 2);

            var translateMtx = Util.TranslationMatrix(spriteCenter.X - spriteOffset.X, renderer.SheetSize - (spriteCenter.Y - spriteOffset.Y), 0);
            var shadowTranslateMtx = Util.TranslationMatrix(shadowCenter.X - shadowSpriteOffset.X, renderer.SheetSize - (shadowCenter.Y - shadowSpriteOffset.Y), 0);
            var correctionTransform = Util.MatrixMultiply(translateMtx, FlipMtx);
            var shadowCorrectionTransform = Util.MatrixMultiply(shadowTranslateMtx, ShadowScaleFlipMtx);

            doRender.Add(Pair.New<Sheet, Action>(sprite.Sheet, () =>
            {

                foreach(var m in models)
                {
                    var offsetVec = Util.MatrixVectorMultiply(invCameraTransform, wr.ScreenVector(m.OffsetFunc()));
                    var offsetTransform = Util.TranslationMatrix(offsetVec[0], offsetVec[1], offsetVec[2]);

                    var rotations = m.RotationFunc().Aggregate(Util.IdentityMatrix(), (x, y) => Util.MatrixMultiply(Util.MakeFloatMatrix(y.AsMatrix()), x));

                    var worldTransform = Util.MatrixMultiply(scaleTransform, rotations);
                    worldTransform = Util.MatrixMultiply(offsetTransform, worldTransform);

                    var transform = Util.MatrixMultiply(cameraTransform, worldTransform);
                    transform = Util.MatrixMultiply(correctionTransform, transform);

                    var shadow = Util.MatrixMultiply(shadowTransform, worldTransform);
                    shadow = Util.MatrixMultiply(shadowCorrectionTransform, shadow);

                    var lightTransform = Util.MatrixMultiply(Util.MatrixInverse(rotations), invShadowTransform);

                    var frame = m.FrameFunc();

                    for(uint i = 0; i < m.Model.Sections; i++)
                    {
                        var rd = m.Model.RenderData(i);
                        var t = m.Model.TransformationMatrix(i, frame);
                        var it = Util.MatrixInverse(t);
                        if (it == null)
                            throw new InvalidOperationException("Failed to invert the transformed matrix of frame {0} during RenderAsync.".F(i));

                        // Transform light vector from shadow -> world -> limb coords
                        var lightDirection = ExtractRotationVector(Util.MatrixMultiply(it, lightTransform));

                        Render(rd, wr.World.ModelCache, Util.MatrixMultiply(shadow, t), lightDirection, lightAmbientColor, lightDiffuseColor, color.TextureMidIndex, normals.TextureMidIndex);

                        //Disable shadow normals by forcing zero diffuse and identity ambient light
                        if (m.ShowShadow)
                            Render(rd, wr.World.ModelCache, Util.MatrixMultiply(shadow, t), lightDirection, ShadowAmbient, ShadowDiffuse, shadowPalette.TextureMidIndex, normals.TextureMidIndex);
                    }
                }


            }));


            var screenLightVector = Util.MatrixVectorMultiply(invShadowTransform, ZVector);
            screenLightVector = Util.MatrixVectorMultiply(cameraTransform, screenLightVector);
            return new ModelRenderProxy(sprite,shadowSprite,screenCorners,-screenLightVector[2]/screenLightVector[1]);
        }

        static float[] ExtractRotationVector(float[] mtx)
        {
            var tVec = Util.MatrixVectorMultiply(mtx, ZVector);
            var tOrigin = Util.MatrixVectorMultiply(mtx, ZeroVector);

            tVec[0] -= tOrigin[0] * tVec[3] / tOrigin[3];
            tVec[1] -= tOrigin[1] * tVec[3] / tOrigin[3];
            tVec[2] -= tOrigin[2] * tVec[3] / tOrigin[3];

            //Renormalize

            var w = (float)Math.Sqrt(tVec[0] * tVec[0] + tVec[1] * tVec[1] + tVec[2] * tVec[2]);
            tVec[0] /= w;
            tVec[1] /= w;
            tVec[2] /= w;
            tVec[3] = 1f;

            return tVec;


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

            sheetBuilder = new SheetBuilder(SheetT.BGRA, AllocateSheet);
            doRender.Clear();    
        }


        public void EndFrame()
        {
            if (doRender.Count == 0)
                return;

            Sheet currentSheet = null;
            IFrameBuffer fbo = null;
            foreach(var v in doRender)
            {
                //Change sheet
                if(v.First != currentSheet)
                {
                    if (fbo != null)
                        DisableFrameBuffer(fbo);

                    currentSheet = v.First;
                    fbo = EnableFrameBuffer(currentSheet);
                }

                v.Second();
            }

            if (fbo != null)
                DisableFrameBuffer(fbo);
        }

        IFrameBuffer EnableFrameBuffer(Sheet s)
        {
            var fbo = mappedBuffers[s];
            WarGame.Renderer.Flush();
            fbo.Bind();

            WarGame.Renderer.Device.EnableDepthBuffer();

            return fbo;
        }

        void DisableFrameBuffer(IFrameBuffer fbo)
        {
            WarGame.Renderer.Flush();
            WarGame.Renderer.Device.DisableDepthBuffer();
            fbo.Unbind();
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
            var frameBuffer = renderer.Device.CreateFrameBuffer(size);
            var sheet = new Sheet(SheetT.BGRA, frameBuffer.Texture);
            mappedBuffers.Add(sheet, frameBuffer);
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
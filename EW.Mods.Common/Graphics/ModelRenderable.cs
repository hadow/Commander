using System;
using System.Collections.Generic;
using System.Linq;
using EW.Graphics;
using EW.Primitives;
using EW.OpenGLES;
namespace EW.Mods.Common.Graphics
{
    public struct ModelRenderable:IRenderable
    {
        readonly IEnumerable<ModelAnimation> models;

        readonly WPos pos;

        readonly int zOffset;

        readonly WRot camera;

        readonly WRot lightSource;

        readonly float[] lightAmbientColor;

        readonly float[] lightDiffuseColor;

        readonly PaletteReference palette;

        readonly PaletteReference normalsPalette;

        readonly PaletteReference shadowPalette;

        readonly float scale;


        public ModelRenderable(IEnumerable<ModelAnimation> models,
                                WPos pos,int zOffset,WRot camera,
                                float scale,WRot lightSource,
                                float[] lightAmbientColor,
                                float[] lightDiffuseColor,
                                PaletteReference color,
                                PaletteReference normals,
                                PaletteReference shadow)
        {
            this.models = models;
            this.pos = pos;
            this.zOffset = zOffset;
            this.scale = scale;
            this.camera = camera;
            this.lightSource = lightSource;
            this.lightAmbientColor = lightAmbientColor;
            this.lightDiffuseColor = lightDiffuseColor;
            palette = color;
            normalsPalette = normals;
            shadowPalette = shadow;


        }

        public WPos Pos { get { return pos; } }

        public PaletteReference Palette { get { return palette; } }

        public int ZOffset { get { return zOffset; } }

        public bool IsDecoration { get { return false; } }

        public IRenderable WithPalette(PaletteReference newPalette)
        {
            return new ModelRenderable(models, pos, zOffset, camera, scale, lightSource, lightAmbientColor, lightDiffuseColor, newPalette, normalsPalette, shadowPalette);
        }

        public IRenderable WithZOffset(int newOffset)
        {
            return new ModelRenderable(models, pos, newOffset, camera, scale, lightSource, lightAmbientColor, lightDiffuseColor, palette, normalsPalette, shadowPalette);
        }

        public IRenderable OffsetBy(WVec vec)
        {
            return new ModelRenderable(models, pos + vec, zOffset, camera, scale, lightSource, lightAmbientColor, lightDiffuseColor, palette, normalsPalette, shadowPalette);
        }

        public IRenderable AsDecoration() { return this; }

        static readonly float[] GroundNormal = new float[] { 0, 0, 1, 1 };

        public IFinalizedRenderable PrepareRender(WorldRenderer wr)
        {
            return new FinalizedModelRenderable(wr, this);
        }

        /// <summary>
        /// 
        /// </summary>
        struct FinalizedModelRenderable : IFinalizedRenderable
        {
            readonly ModelRenderable model;
            readonly ModelRenderProxy renderProxy;

            public FinalizedModelRenderable(WorldRenderer wr,ModelRenderable model)
            {
                this.model = model;
                var draw = model.models.Where(v => v.DisableFunc == null || !v.DisableFunc());

                renderProxy = WarGame.Renderer.WorldModelRenderer.RenderAsync(wr, draw, model.camera, model.scale, GroundNormal, model.lightSource, model.lightAmbientColor, model.lightDiffuseColor, model.palette, model.normalsPalette, model.shadowPalette);
            }

            public void Render(WorldRenderer wr)
            {
                var groundPos = model.pos - new WVec(0, 0, wr.World.Map.DistanceAboveTerrain(model.pos).Length);
                var tileScale = wr.World.Map.Grid.Type == MapGridT.RectangularIsometric ? 1448f : 1024f;

                var groundZ = wr.World.Map.Grid.TileSize.Height * (groundPos.Z - model.pos.Z) / tileScale;
                var pxOrigin = wr.Screen3DPosition(model.pos);

                //HACK:We don't have enough texture channels to pass the depth data to the shader
                //so for now just offset everything forward so that the back corner is rendered at pos.
                pxOrigin = new Vector3(0, 0, Screen3DBounds(wr).Second.X);

                var shadowOrigin = pxOrigin - groundZ * (new Vector2(renderProxy.ShadowDirection, 1));

                var psb = renderProxy.ProjectedShadowBounds;
                var sa = shadowOrigin + psb[0];
                var sb = shadowOrigin + psb[2];
                var sc = shadowOrigin + psb[1];
                var sd = shadowOrigin + psb[3];

            }

            public void RenderDebugGeometry(WorldRenderer wr)
            {

            }


            public Rectangle ScreenBounds(WorldRenderer wr)
            {
                return Screen3DBounds(wr).First;
            }

            Pair<Rectangle,Vector2> Screen3DBounds(WorldRenderer wr)
            {
                var pxOrigin = wr.ScreenPosition(model.pos);

                var draw = model.models.Where(v => v.DisableFunc != null || !v.DisableFunc());

                var scaleTransform = EW.Graphics.Util.ScaleMatrix(model.scale, model.scale, model.scale);
                var cameraTransform = EW.Graphics.Util.MakeFloatMatrix(model.camera.AsMatrix());

                var minX = float.MaxValue;
                var minY = float.MaxValue;
                var minZ = float.MaxValue;
                var maxX = float.MinValue;
                var maxY = float.MinValue;
                var maxZ = float.MinValue;

                return Pair.New(Rectangle.FromLTRB((int)minX, (int)minY, (int)maxX, (int)maxY), new Vector2(minZ, maxZ));
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using EW.Graphics;
using EW.Primitives;
using EW.Framework;
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


            static readonly uint[] CornerXIndex = new uint[] { 0, 0, 0, 0, 3, 3, 3, 3 };
            static readonly uint[] CornerYIndex = new uint[] { 1, 1, 4, 4, 1, 1, 4, 4 };
            static readonly uint[] CornerZIndex = new uint[] { 2, 5, 2, 5, 2, 5, 2, 5 };
            public FinalizedModelRenderable(WorldRenderer wr,ModelRenderable model)
            {
                this.model = model;
                var draw = model.models.Where(v => v.DisableFunc == null || !v.DisableFunc());

                renderProxy = WarGame.Renderer.WorldModelRenderer.RenderAsync(wr, draw, model.camera, model.scale, GroundNormal, model.lightSource,
                    model.lightAmbientColor, model.lightDiffuseColor,
                    model.palette, model.normalsPalette, model.shadowPalette);
            }

            public void Render(WorldRenderer wr)
            {
                var groundPos = model.pos - new WVec(0, 0, wr.World.Map.DistanceAboveTerrain(model.pos).Length);
                var tileScale = wr.World.Map.Grid.Type == MapGridT.RectangularIsometric ? 1448f : 1024f;

                var groundZ = wr.World.Map.Grid.TileSize.Height * (groundPos.Z - model.pos.Z) / tileScale;
                var pxOrigin = wr.Screen3DPosition(model.pos);

                //HACK:We don't have enough texture channels to pass the depth data to the shader
                //so for now just offset everything forward so that the back corner is rendered at pos.
                //我们没有足够的纹理通道将深度数据传递给着色器，所以现在只需将所有东西都向前偏移，以便后边的角落渲染位置
                pxOrigin -= new Vector3(0, 0, Screen3DBounds(wr).Second.X);

                var shadowOrigin = pxOrigin - groundZ * (new Vector2(renderProxy.ShadowDirection, 1));

                var psb = renderProxy.ProjectedShadowBounds;
                var sa = shadowOrigin + psb[0];
                var sb = shadowOrigin + psb[2];
                var sc = shadowOrigin + psb[1];
                var sd = shadowOrigin + psb[3];

                WarGame.Renderer.WorldRgbaSpriteRenderer.DrawSprite(renderProxy.ShadowSprite, sa, sb, sc, sd);
                WarGame.Renderer.WorldRgbaSpriteRenderer.DrawSprite(renderProxy.Sprite, pxOrigin - 0.5f * renderProxy.Sprite.Size);
            }

            public void RenderDebugGeometry(WorldRenderer wr)
            {
                var groundPos = model.pos - new WVec(0, 0, wr.World.Map.DistanceAboveTerrain(model.pos).Length);
                var groundZ = wr.World.Map.Grid.TileSize.Height * (groundPos.Z - model.pos.Z) / 1024f;
                var pxOrigin = wr.Screen3DPosition(model.pos);
                var shadowOrigin = pxOrigin - groundZ * (new Vector2(renderProxy.ShadowDirection, 1));
                var iz = 1 / wr.ViewPort.Zoom;

                // Draw sprite rect
                var offset = pxOrigin + renderProxy.Sprite.Offset - 0.5f * renderProxy.Sprite.Size;
                WarGame.Renderer.WorldRgbaColorRenderer.DrawRect(offset.XY, (offset + renderProxy.Sprite.Size).XY, iz, Color.Red);

                // Draw transformed shadow sprite rect
                var c = Color.Purple;
                var psb = renderProxy.ProjectedShadowBounds;

                WarGame.Renderer.WorldRgbaColorRenderer.DrawPolygon(new[]
                {
                    shadowOrigin + psb[1],
                    shadowOrigin + psb[3],
                    shadowOrigin + psb[0],
                    shadowOrigin + psb[2]
                }, iz, c);

                // Draw bounding box
                var draw = model.models.Where(v => v.IsVisible);
                var scaleTransform = EW.Graphics.Util.ScaleMatrix(model.scale, model.scale, model.scale);
                var cameraTransform = EW.Graphics.Util.MakeFloatMatrix(model.camera.AsMatrix());

                foreach (var v in draw)
                {
                    var bounds = v.Model.Bounds(v.FrameFunc());
                    var worldTransform = v.RotationFunc().Reverse().Aggregate(scaleTransform,
                        (x, y) => EW.Graphics.Util.MatrixMultiply(x, EW.Graphics.Util.MakeFloatMatrix(y.AsMatrix())));

                    var pxPos = pxOrigin + wr.ScreenVectorComponents(v.OffsetFunc());
                    var screenTransform = EW.Graphics.Util.MatrixMultiply(cameraTransform, worldTransform);
                    DrawBoundsBox(pxPos, screenTransform, bounds, iz, Color.Yellow);
                }
            }

            static void DrawBoundsBox(Vector3 pxPos, float[] transform, float[] bounds, float width, Color c)
            {
                var wcr = WarGame.Renderer.WorldRgbaColorRenderer;
                var corners = new Vector3[8];
                for (var i = 0; i < 8; i++)
                {
                    var vec = new float[] { bounds[CornerXIndex[i]], bounds[CornerYIndex[i]], bounds[CornerZIndex[i]], 1 };
                    var screen = EW.Graphics.Util.MatrixVectorMultiply(transform, vec);
                    corners[i] = pxPos + new Vector3(screen[0], screen[1], screen[2]);
                }

                // Front face
                wcr.DrawPolygon(new[] { corners[0], corners[1], corners[3], corners[2] }, width, c);

                // Back face
                wcr.DrawPolygon(new[] { corners[4], corners[5], corners[7], corners[6] }, width, c);

                // Horizontal edges
                wcr.DrawLine(corners[0], corners[4], width, c);
                wcr.DrawLine(corners[1], corners[5], width, c);
                wcr.DrawLine(corners[2], corners[6], width, c);
                wcr.DrawLine(corners[3], corners[7], width, c);
            }

            public Rectangle ScreenBounds(WorldRenderer wr)
            {
                return Screen3DBounds(wr).First;
            }

            Pair<Rectangle,Vector2> Screen3DBounds(WorldRenderer wr)
            {
                var pxOrigin = wr.ScreenPosition(model.pos);

                var draw = model.models.Where(v => v.IsVisible);

                var scaleTransform = EW.Graphics.Util.ScaleMatrix(model.scale, model.scale, model.scale);
                var cameraTransform = EW.Graphics.Util.MakeFloatMatrix(model.camera.AsMatrix());

                var minX = float.MaxValue;
                var minY = float.MaxValue;
                var minZ = float.MaxValue;
                var maxX = float.MinValue;
                var maxY = float.MinValue;
                var maxZ = float.MinValue;

                foreach(var v in draw)
                {
                    var bounds = v.Model.Bounds(v.FrameFunc());
                    var worldTransform = v.RotationFunc().Reverse().Aggregate(scaleTransform,
                        (x, y) => EW.Graphics.Util.MatrixMultiply(x, EW.Graphics.Util.MakeFloatMatrix(y.AsMatrix())));

                    var pxPos = pxOrigin + wr.ScreenVectorComponents(v.OffsetFunc());
                    var screenTransform = EW.Graphics.Util.MatrixMultiply(cameraTransform, worldTransform);

                    for(var i = 0; i < 8; i++)
                    {
                        var vec = new float[] { bounds[CornerXIndex[i]], bounds[CornerYIndex[i]], bounds[CornerZIndex[i]], 1 };
                        var screen = EW.Graphics.Util.MatrixVectorMultiply(screenTransform, vec);
                        minX = Math.Min(minX, pxPos.X + screen[0]);
                        minY = Math.Min(minY, pxPos.Y + screen[1]);
                        minZ = Math.Min(minZ, pxPos.Z + screen[2]);
                        maxX = Math.Max(maxX, pxPos.X + screen[0]);
                        maxY = Math.Max(maxY, pxPos.Y + screen[1]);
                        maxZ = Math.Max(maxZ, pxPos.Z + screen[2]);
                    }

                }

                return Pair.New(Rectangle.FromLTRB((int)minX, (int)minY, (int)maxX, (int)maxY), new Vector2(minZ, maxZ));
            }
        }
    }
}
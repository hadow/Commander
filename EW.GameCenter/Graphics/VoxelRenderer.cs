using System;
using System.Collections.Generic;
using EW.Xna.Platforms;
using EW.Primitives;
using EW.Xna.Platforms.Graphics;
namespace EW.Graphics
{
    public class VoxelRenderProxy
    {
        public readonly Sprite Sprite;
        public readonly Sprite ShadowSprite;
        public readonly float ShadowDirection;
        public readonly Vector3[] ProjectedShadowBounds;

        public VoxelRenderProxy(Sprite sprite,Sprite shadowSprite,Vector3[] projectedShadowBounds,float shadowDirection)
        {
            Sprite = sprite;
            ShadowSprite = shadowSprite;
            ProjectedShadowBounds = projectedShadowBounds;
            ShadowDirection = shadowDirection;
        }
    }


    public sealed class VoxelRenderer
    {
        static readonly float[] ShadowDiffuse = new float[] { 0, 0, 0 };


        readonly List<Pair<Sheet, Action>> doRender = new List<Pair<Sheet, Action>>();

        readonly Renderer renderer;

        readonly Effect shader;

        SheetBuilder sheetBuilder;

        public VoxelRenderer(Renderer renderer,Effect shader)
        {
            this.renderer = renderer;
        }

        public void SetPalette(Texture palette)
        {
            shader.Parameters["Palette"].SetValue(palette);
        }

        public void SetViewportParams(float zoom,Point scroll)
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

        void Render(VoxelRenderData renderData,float[] t,float[] lightDirection,float[] ambientLight)

        public void BeginFrame()
        {

        }





    }
}
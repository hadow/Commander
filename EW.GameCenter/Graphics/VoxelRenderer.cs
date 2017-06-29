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
        void Render(VoxelRenderData renderData,float[] t,float[] lightDirection,
            float[] ambientLight,float[] diffuseLight,float colorPaletteTextureMidIndex,float normalsPaletteTextureMidIndex)
        {
            shader.Parameters["DiffuseTexture"].SetValue(renderData.Sheet.GetTexture());
            shader.Parameters["PaletteRows"].SetValue(new Vector2(colorPaletteTextureMidIndex, normalsPaletteTextureMidIndex));
            //shader.Parameters["TransformMatrix"].SetValue(new Matrix());
            //shader.Parameters["LightDirection"].set
        }

        public void BeginFrame()
        {

        }





    }
}
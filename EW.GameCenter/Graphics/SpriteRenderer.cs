using System;
using System.Drawing;
using EW.Xna.Platforms.Graphics;
using EW.Xna.Platforms;
namespace EW.Graphics
{
    public class SpriteRenderer
    {
        readonly Renderer renderer;

        //readonly Action renderAction;

        readonly Effect effect;

        readonly Vertex[] vertices;
        Sheet currentSheet;

        BlendMode currentBlend = BlendMode.Alpha;

        int nv = 0;

        public SpriteRenderer(Renderer renderer,Effect effect)
        {
            this.renderer = renderer;
            this.effect = effect;
            vertices = new Vertex[renderer.TempBufferSize];
            //renderAction = () => renderer.DrawBatch(vertices, nv, PrimitiveType.TriangleList);
        }

        public void DrawVertexBuffer(DynamicVertexBuffer buffer,int start,int length,PrimitiveType type,Sheet sheet,BlendMode blendMode)
        {
            effect.Parameters["DiffuseTexture"].SetValue(sheet.GetTexture());

            effect.CurrentTechnique.Passes[0].Apply();

            renderer.GraphicsDevice.DrawPrimitives(type, start, length);
        }

        public void SetPalette(Texture palette)
        {
            effect.Parameters["Palette"].SetValue(palette);
        }

        public void SetViewportParams(Size screen,float depthScale,float depthOffset,float zoom,EW.Xna.Platforms.Point scroll)
        {
            effect.Parameters["Scroll"].SetValue(new Vector3(scroll.X, scroll.Y, scroll.Y));
            effect.Parameters["r1"].SetValue(new Vector3(zoom * 2f / screen.Width, -zoom * 2f / screen.Height, -depthScale * zoom / screen.Height));
            effect.Parameters["r2"].SetValue(new Vector4(-1,1,1,-depthOffset));
            effect.Parameters["DepthTextureScale"].SetValue(128 * depthScale * zoom / screen.Height);
        }

        public void SetDepthPreviewEnabled(bool enabled)
        {
            effect.Parameters["EnableDepthPreview"].SetValue(enabled);
        }

        public void Flush()
        {

        }




    }
}
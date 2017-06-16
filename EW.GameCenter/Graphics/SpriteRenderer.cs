using System;
using EW.Xna.Platforms.Graphics;
namespace EW.Graphics
{
    public class SpriteRenderer
    {
        readonly Renderer renderer;

        readonly Action renderAction;

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
            renderAction = () => renderer.DrawBatch(vertices, nv, PrimitiveType.TriangleList);
        }
        public void DrawVertexBuffer(DynamicVertexBuffer buffer,int start,int length,PrimitiveType type,Sheet sheet,BlendMode blendMode)
        {

        }



        public void Flush()
        {

        }




    }
}
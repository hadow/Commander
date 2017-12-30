using System;
using System.Drawing;
using EW.OpenGLES;
using EW.OpenGLES.Graphics;
namespace EW.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    public class RgbaColorRenderer:Renderer.IBatchRenderer
    {

        static readonly Vector2 Offset = new Vector2(0.5f, 0.5f);

        readonly Renderer renderer;

        readonly IShader shader;

        readonly Vertex[] vertices;

        readonly Action renderAction;
        int nv = 0;

        public RgbaColorRenderer(Renderer renderer,IShader shader)
        {
            this.renderer = renderer;
            this.shader = shader;
            vertices = new Vertex[renderer.TempBufferSize];
            renderAction = () => renderer.DrawBatch(vertices, nv, PrimitiveType.TriangleList);
        }

        public void Flush()
        {
            if (nv > 0)
            {
                renderer.Device.SetBlendMode(BlendMode.Alpha);
                shader.Render(renderAction);
                renderer.Device.SetBlendMode(BlendMode.None);

                nv = 0;
            }
        }

        public void DrawRect(Vector3 tl,Vector3 br,float width,Color color){

            var tr = new Vector3(br.X, tl.Y, tl.Z);
            var bl = new Vector3(tl.X, br.Y, br.Z);



        }

        public void DrawLine(Vector3 start,Vector3 end,float width,Color startColor,Color endColor)
        {
            renderer.CurrentBatchRenderer = this;

            if (nv + 6 > renderer.TempBufferSize)
                Flush();

            var delta = (end - start) / (end - start).XY.Length;
            var corner = width / 2 * new Vector3(-delta.Y, delta.X, delta.Z);

            startColor = Util.PremultiplyAlpha(startColor);

            var sr = startColor.R / 255.0f;
            var sg = startColor.G / 255.0f;
            var sb = startColor.B / 255.0f;
            var sa = startColor.A / 255.0f;

            endColor = Util.PremultiplyAlpha(endColor);

            var er = endColor.R / 255.0f;
            var eg = endColor.G / 255.0f;
            var eb = endColor.B / 255.0f;
            var ea = endColor.A / 255.0f;

            vertices[nv++] = new Vertex(start - corner + Offset, sr, sg, sb, sa, 0, 0);
            vertices[nv++] = new Vertex(start + corner + Offset, sr, sg, sb, sa, 0, 0);
            vertices[nv++] = new Vertex(end + corner + Offset, er, eg, eb, ea, 0, 0);
            vertices[nv++] = new Vertex(end + corner + Offset, er, eg, eb, ea, 0, 0);
            vertices[nv++] = new Vertex(end - corner + Offset, er, eg, eb, ea, 0, 0);
            vertices[nv++] = new Vertex(start - corner + Offset, sr, sg, sb, sa, 0, 0);
        }

        public void DrawLine(Vector3 start,Vector3 end,float width,Color color)
        {
            renderer.CurrentBatchRenderer = this;
            if (nv + 6 > renderer.TempBufferSize)
                Flush();

            var delta = (end - start) / (end - start).XY.Length;
            var corner = width / 2 * new Vector2(-delta.Y, delta.X);

            color = Util.PremultiplyAlpha(color);
            var r = color.R / 255.0f;
            var g = color.G / 255.0f;
            var b = color.B / 255.0f;
            var a = color.A / 255.0f;

            vertices[nv++] = new Vertex(start - corner + Offset, r, g, b, a, 0, 0);
            vertices[nv++] = new Vertex(start + corner + Offset, r, g, b, a, 0, 0);
            vertices[nv++] = new Vertex(end + corner + Offset, r, g, b, a, 0, 0);
            vertices[nv++] = new Vertex(end + corner + Offset, r, g, b, a, 0, 0);
            vertices[nv++] = new Vertex(end - corner + Offset, r, g, b, a, 0, 0);
            vertices[nv++] = new Vertex(start - corner + Offset, r, g, b, a, 0, 0);
        }


        public void SetViewportParams(Size screen,float depthScale,float depthOffset,float zoom,Int2 scroll)
        {
            shader.SetVec("Scroll",scroll.X,scroll.Y,scroll.Y);
            shader.SetVec("r1",zoom*2f/screen.Width,-zoom*2f/screen.Height,-depthScale*zoom/screen.Height);
            shader.SetVec("r2",-1,1,1-depthOffset);
        }

        public void SetDepthPreviewEnabled(bool enabled)
        {
            shader.SetBool("EnableDepthPreview",enabled);
        }
    }
}
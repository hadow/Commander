using System;
using System.Drawing;
using EW.OpenGLES;
using EW.OpenGLES.Graphics;
namespace EW.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    public class RgbaColorRenderer
    {

        static readonly Vector2 Offset = new Vector2(0.5f, 0.5f);

        readonly Renderer renderer;

        readonly IShader shader;

        readonly Vertex[] vertices;

        public RgbaColorRenderer(Renderer renderer,IShader shader)
        {
            this.renderer = renderer;
            this.shader = shader;
        }


        public void DrawRect(Vector3 tl,Vector3 br,float width,EW.OpenGLES.Color color){

            var tr = new Vector3(br.X, tl.Y, tl.Z);
            var bl = new Vector3(tl.X, br.Y, br.Z);



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
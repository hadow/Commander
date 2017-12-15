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


        public void SetViewportParams(Size screen,float depthScale,float depthOffset,float zoom,Int2 scroll)
        {

        }

        public void SetDepthPreviewEnabled(bool enabled)
        {

        }
    }
}
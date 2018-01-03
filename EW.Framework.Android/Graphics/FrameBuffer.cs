using System;
using System.Drawing;
using System.IO;

namespace EW.Framework.Graphics
{
    sealed class FrameBuffer:IFrameBuffer
    {

        readonly Texture texture;
        readonly Size size;

        int framebuffer, depth;

        bool disposed;


        int[] cv = new int[4];
        public FrameBuffer(Size size)
        {
            this.size = size;

            if (!Exts.IsPowerOf2(size.Width) || !Exts.IsPowerOf2(size.Height))
                throw new InvalidDataException(string.Format("Frame buffer size ({0}x{1}) must be a power of two", size.Width, size.Height));

            GL.GenFramebuffers(1, out framebuffer);
            GraphicsExtensions.CheckGLError();
            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, framebuffer);
            GraphicsExtensions.CheckGLError();

            //Color
            texture = new Graphics.Texture();
            texture.SetEmpty(size.Width, size.Height);
            GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, texture.ID, 0);
            GraphicsExtensions.CheckGLError();

            //Depth
            GL.GenRenderbuffers(1, out depth);
            GraphicsExtensions.CheckGLError();

            GL.BindRenderbuffer(RenderbufferTarget.RenderbufferExt, depth);
            GraphicsExtensions.CheckGLError();

            GL.RenderbufferStorage(RenderbufferTarget.RenderbufferExt, RenderbufferStorage.DepthComponent24Oes, size.Width, size.Height);
            GraphicsExtensions.CheckGLError();

            GL.FramebufferRenderbuffer(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachment, RenderbufferTarget.RenderbufferExt, depth);
            GraphicsExtensions.CheckGLError();
            //Test for completeness
            var status = GL.CheckFramebufferStatus(FramebufferTarget.FramebufferExt);
            if(status != FramebufferErrorCode.FramebufferCompleteExt)
            {
                throw new InvalidOperationException("OpenGL Error:"+status);
            }

            //Restore default buffer
            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
            GraphicsExtensions.CheckGLError();

        }


        static int[] ViewportRectangle()
        {
            var v = new int[4];
            unsafe
            {
                fixed (int* ptr = &v[0])
                    GL.GetIntegerv(0x0BA2, ptr);
            }

            GraphicsExtensions.CheckGLError();

            return v;
        }

        public void Bind()
        {
            Threading.EnsureUIThread();

            //Cache viewport rect to restore when unbinding
            cv = ViewportRectangle();

            GL.Flush();
            GraphicsExtensions.CheckGLError();
            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, framebuffer);
            GraphicsExtensions.CheckGLError();
            GL.Viewport(0, 0, size.Width, size.Height);
            GraphicsExtensions.CheckGLError();
            GL.ClearColor(0, 0, 0, 0);
            GraphicsExtensions.CheckGLError();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GraphicsExtensions.CheckGLError();

        }

        public void Unbind()
        {
            Threading.EnsureUIThread();
            GL.Flush();
            GraphicsExtensions.CheckGLError();
            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
            GraphicsExtensions.CheckGLError();
            GL.Viewport(cv[0], cv[1], cv[2], cv[3]);
            GraphicsExtensions.CheckGLError();
        }

        public ITexture Texture
        {
            get
            {
                Threading.EnsureUIThread();
                return texture;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        void Dispose(bool disposing)
        {
            if (disposed)
                return;
            disposed = true;
            if (disposing)
                texture.Dispose();

            GL.DeleteFramebuffers(1, ref framebuffer);
            GraphicsExtensions.CheckGLError();
            GL.DeleteRenderbuffers(1, ref depth);
            GraphicsExtensions.CheckGLError();
        }

    }
}
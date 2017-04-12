using System;
using System.Runtime.InteropServices;
#if GLES
using OpenTK.Graphics.ES20;
using System.Security;
#endif

namespace EW.Xna.Platforms.Graphics
{
    partial class GraphicsDevice
    {
#if GLES
        internal class FramebufferHelper
        {
            public bool SupportsInvalidateFramebuffer { get; private set; }

            public bool SupportsBlitFramebuffer { get; private set; }

#if IOS
#elif ANDROID
            internal const string OpenGLLibrary = "libGLESv2.dll";
            [DllImport("libEGL.dll",EntryPoint ="eglGetProcAddress")]
            public static extern IntPtr EGLGetProcAddress(string funcname);
#endif


            internal FramebufferHelper(GraphicsDevice graphicsDevice)
            {
#if IOS
#elif ANDROID


#endif
            }

            internal virtual void GenFramebuffer(out int framebuffer)
            {
                framebuffer = 0;

#if (ANDROID || IOS)
                GL.GenFramebuffers(1, out framebuffer);
#endif
            }

            internal virtual void BindFramebuffer(int framebuffer)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
                GraphicsExtensions.CheckGLError();
            }
            internal virtual void DeleteFramebuffer(int framebuffer)
            {
                GL.DeleteFramebuffers(1, ref framebuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void GenRenderbuffer(out int renderbuffer)
            {
                renderbuffer = 0;
#if (ANDROID || IOS)
                GL.GenRenderbuffers(1, out renderbuffer);
#endif
                GraphicsExtensions.CheckGLError();

            }

            internal virtual void BindRenderbuffer(int renderbuffer)
            {
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbuffer);
                GraphicsExtensions.CheckGLError();
            }


            internal virtual void DeleteRenderbuffer(int renderbuffer)
            {
                GL.DeleteRenderbuffers(1, ref renderbuffer);
                GraphicsExtensions.CheckGLError();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="attachement"></param>
            /// <param name="target"></param>
            /// <param name="texture"></param>
            /// <param name="level"></param>
            /// <param name="samples"></param>
            internal virtual void FramebufferTexture2D(int attachement, int target, int texture, int level = 0,int samples = 0)
            {
                if (samples > 0)
                {
                    
                }
                else
                {
                    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, (FramebufferSlot)attachement, (TextureTarget)target, texture, level);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="attachement"></param>
            /// <param name="renderbuffer"></param>
            /// <param name="level"></param>
            internal virtual void FramebufferRenderbuffer(int attachement,int renderbuffer,int level = 0)
            {
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, (FramebufferSlot)attachement, RenderbufferTarget.Renderbuffer, renderbuffer);
                GraphicsExtensions.CheckGLError();
            }
        }
#endif

    }
}
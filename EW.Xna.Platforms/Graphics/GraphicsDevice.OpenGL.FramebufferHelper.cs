using System;
using System.Runtime.InteropServices;
using Android.Util;
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

            private static FramebufferHelper _instance;

            public static FramebufferHelper Create(GraphicsDevice gd)
            {
                if (gd.GraphicsCapabilities.SupportsFramebufferObjectARB)
                {
                    _instance = new FramebufferHelper(gd);
                }
                else
                {

                }
                return _instance;
            }
#if IOS


#elif ANDROID
            internal const string OpenGLLibrary = "libGLESv2.dll";
            [DllImport("libEGL.dll",EntryPoint ="eglGetProcAddress")]
            public static extern IntPtr EGLGetProcAddress(string funcname);
#endif

            internal delegate void GLInvalidateFramebufferDelegate(All target, int numAttachments, All[] attachments);

            internal delegate void GLRenderbufferStorageMultisampleDelegate(All target, int samples, All internalFormat, int width, int height);

            internal delegate void GLBlitFramebufferDelegate(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, ClearBufferMask mask, TextureMagFilter filter);

            internal GLInvalidateFramebufferDelegate GLInvalidateFramebuffer;
            internal GLRenderbufferStorageMultisampleDelegate GLRenderbufferStorageMultisample;
            internal GLBlitFramebufferDelegate GLBlitFramebuffer;

            internal All AllReadFramebuffer = All.Framebuffer;
            internal All AllDrawFramebuffer = All.Framebuffer;
            internal FramebufferHelper(GraphicsDevice graphicsDevice)
            {
#if IOS


#elif ANDROID
                var invalidFuncPtr = EGLGetProcAddress("InvalidFunctionName");

                if (graphicsDevice._extensions.Contains("GL_EXT_discard_framebuffer"))
                {
                    Log.Debug("extensions","GL_EXT_discard_framebuffer");
                }

                if (graphicsDevice._extensions.Contains("GL_EXT_multisampled_render_to_texture"))
                {



                    Log.Debug("extensions", "GL_EXT_multisampled_render_to_texture");
                }
                else if (graphicsDevice._extensions.Contains("GL_IMG_multisampled_render_to_texture"))
                {
                    Log.Debug("extensions", "GL_IMG_multisampled_render_to_texture");
                }
                else if (graphicsDevice._extensions.Contains("GL_NV_framebuffer_multisample"))
                {
                    Log.Debug("extensions", "GL_NV_framebuffer_multisample");
                }

#endif
            }

            internal readonly All[] GLDiscardAttachements = { All.ColorAttachment0, All.DepthAttachment, All.StencilAttachment, };

            internal virtual void InvalidateDrawFramebuffer()
            {
                this.GLInvalidateFramebuffer(AllDrawFramebuffer, 3, GLDiscardAttachements);
            }

            internal virtual void InvalidateReadFramebuffer()
            {
                
                this.GLInvalidateFramebuffer(AllReadFramebuffer, 3, GLDiscardAttachements);
            }

            internal virtual void BlitFramebuffer(int iColorAttachment,int width,int height)
            {
                this.GLBlitFramebuffer(0, 0, width, height, 0, 0, width, height, ClearBufferMask.ColorBufferBit, TextureMagFilter.Nearest);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void GenFramebuffer(out int framebuffer)
            {
                framebuffer = 0;

#if (ANDROID || IOS)
                GL.GenFramebuffers(1, out framebuffer);
#endif
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void BindReadFramebuffer(int readFramebuffer)
            {
                GL.BindFramebuffer((FramebufferTarget)AllReadFramebuffer, readFramebuffer);
                GraphicsExtensions.CheckGLError();
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


            internal virtual void RenderbufferStorageMultisample(int samples,int internalFormat,int width,int height)
            {
                if (samples > 0 && this.GLRenderbufferStorageMultisample != null)
                    GLRenderbufferStorageMultisample(All.Renderbuffer, samples, (All)internalFormat, width, height);
                else
                    GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, (RenderbufferInternalFormat)internalFormat, width, height);

                GraphicsExtensions.CheckGLError();
            }

            internal virtual void GenerateMipmap(int target)
            {
                GL.GenerateMipmap((TextureTarget)target);
                GraphicsExtensions.CheckGLError();
            }
        }
#endif

    }
}
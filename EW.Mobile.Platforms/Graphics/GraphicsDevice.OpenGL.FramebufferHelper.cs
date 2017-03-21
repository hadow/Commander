using System;
using System.Runtime.InteropServices;
#if GLES
using OpenTK.Graphics.ES20;
using System.Security;
#endif

namespace RA.Mobile.Platforms.Graphics
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
        }
#endif
            }
}
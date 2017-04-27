using System;


namespace EW.Xna.Platforms.Graphics
{
    internal partial class GraphicsCapabilities
    {

        //GL_ARB_framebuffer_object
        internal bool SupportsFramebufferObjectARB { get; private set; }

        //GL_EXT_framebuffer_object
        internal bool SupportsFramebufferObjectEXT { get; private set; }

        internal int MaxTextureAnisotropy { get; private set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        internal void PlatformInitialize(GraphicsDevice device)
        {
            SupportsNonPowerOfTwo = GetNonPowerOfTwo(device);

            SupportsFramebufferObjectARB = true;
            SupportsFramebufferObjectEXT = false;
        }

        bool GetNonPowerOfTwo(GraphicsDevice device)
        {
            return device._extensions.Contains("GL_OES_texture_npot") ||
                    device._extensions.Contains("GL_ARB_texture_non_power_of_two") ||
                    device._extensions.Contains("GL_IMG,texture_npot") ||
                    device._extensions.Contains("GL_NV_texture_npot_2D_mipmap");
        }
    }
}
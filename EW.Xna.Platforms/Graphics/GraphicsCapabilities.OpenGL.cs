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


            //Texture compression
            SupportsS3tc = device._extensions.Contains("GL_EXT_texture_compression_s3tc") ||
                           device._extensions.Contains("GL_OES_texture_compression_S3TC") ||
                           device._extensions.Contains("GL_EXT_texture_compression_dxt3") ||
                           device._extensions.Contains("GL_EXT_texture_compression_dxt5");
            SupportsDxt1 = SupportsS3tc || device._extensions.Contains("GL_EXT_texture_compression_dxt1");

            SupportsPvrtc = device._extensions.Contains("GL_IMG_texture_compression_pvrtc");

            SupportsEtc1 = device._extensions.Contains("GL_OES_compressed_ETC1_RGB8_texture");

            //Framebuffer objects
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
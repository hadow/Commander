using System;
using OpenTK.Graphics.ES20;

namespace EW.Xna.Platforms.Graphics
{
    public partial class SamplerState
    {
        private readonly float[] _openGLBorderColor = new float[4];

#if GLES
        private const TextureParameterName TextureParameterNameTextureMaxAnisotropy = (TextureParameterName)All.TextureMaxAnisotropyExt;
        private const TextureParameterName TextureParameterNameTextureMaxLevel = (TextureParameterName)0x813D;
#else

#endif
        /// <summary>
        /// ¼¤»î
        /// </summary>
        /// <param name="device"></param>
        /// <param name="target"></param>
        /// <param name="useMipmaps"></param>
        internal void Activate(GraphicsDevice device,TextureTarget target,bool useMipmaps = false)
        {
            if (GraphicsDevice == null)
                GraphicsDevice = device;

            switch (Filter)
            {
                case TextureFilter.Point:
                    if (GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                    {
                        GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                        GraphicsExtensions.CheckGLError();
                    }
                    GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.NearestMipmapNearest : TextureMinFilter.Nearest));
                    GraphicsExtensions.CheckGLError();
                    GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    GraphicsExtensions.CheckGLError();
                    break;
                case TextureFilter.Linear:
                    if (GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                    {
                        GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                        GraphicsExtensions.CheckGLError();
                    }
                    GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear));
                    GraphicsExtensions.CheckGLError();
                    GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GraphicsExtensions.CheckGLError();
                    break;
                default:
                    throw new NotSupportedException();
                    
            }

            //Set up texture addressing.
            //ÉèÖÃÎÆÀíÑ°Ö·
            GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)GetWrapMode(AddressU));
            GraphicsExtensions.CheckGLError();
            GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)GetWrapMode(AddressV));
            GraphicsExtensions.CheckGLError();


            if (GraphicsDevice.GraphicsCapabilities.SupportsTextureMaxLevel)
            {
                if(this.MaxMipLevel > 0)
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterNameTextureMaxLevel, this.MaxMipLevel);
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterNameTextureMaxLevel, 1000);
                }
            }
        }

        private int GetWrapMode(TextureAddressMode textureAddressMode)
        {

            switch (textureAddressMode)
            {
                case TextureAddressMode.Clamp:
                    return (int)TextureWrapMode.ClampToEdge;
                case TextureAddressMode.Wrap:
                    return (int)TextureWrapMode.Repeat;
                case TextureAddressMode.Mirror:
                    return (int)TextureWrapMode.MirroredRepeat;
                default:
                    throw new ArgumentException("No Support for" + textureAddressMode);
            }

        }


    }
}
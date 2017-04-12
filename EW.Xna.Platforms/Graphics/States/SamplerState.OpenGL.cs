using System;
using OpenTK.Graphics.ES20;

namespace EW.Xna.Platforms.Graphics
{
    public partial class SamplerState
    {
        private readonly float[] _openGLBorderColor = new float[4];

#if GLES


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
                    GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.NearestMipmapNearest : TextureMinFilter.Nearest));
                    GraphicsExtensions.CheckGLError();
                    GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    GraphicsExtensions.CheckGLError();
                    break;
                case TextureFilter.Linear:
                    GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear));
                    GraphicsExtensions.CheckGLError();
                    GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GraphicsExtensions.CheckGLError();
                    break;
                default:
                    throw new NotSupportedException();
                    
            }

            GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)GetWrapMode(AddressU));
            GraphicsExtensions.CheckGLError();
            GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)GetWrapMode(AddressV));
            GraphicsExtensions.CheckGLError();
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
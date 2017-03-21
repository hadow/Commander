using System;

#if ANDROID
using Android.Graphics;
#endif
#if GLES
using OpenTK.Graphics.ES20;
using GLPixelFormat = OpenTK.Graphics.ES20.All;
using PixelFormat = OpenTK.Graphics.ES20.PixelFormat;
using PixelInternalFormat = OpenTK.Graphics.ES20.PixelFormat;
#endif
namespace EW.Mobile.Platforms.Graphics
{

    public partial class Texture2D:Texture
    {
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mipmap"></param>
        /// <param name="format"></param>
        /// <param name="type"></param>
        /// <param name="shared"></param>
        private void PlatformConstruct(int width,int height,bool mipmap,SurfaceFormat format,SurfaceType type,bool shared)
        {
            this.glTarget = TextureTarget.Texture2D;

            Threading.BlockOnUIThread(() => {

                var prevTexture = GraphicsExtensions.GetBoundTexture2D();

                GenerateGLTextureIfRequired();
                
                if(glFormat == (PixelFormat)GLPixelFormat.CompressedTextureFormats)
                {

                }
                else
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0, glInternalFormat, this.width, this.height, 0, glFormat, glType, IntPtr.Zero);
                    GraphicsExtensions.CheckGLError();
                }

                if (mipmap)
                {
#if IOS || ANDROID
                    GL.GenerateMipmap(TextureTarget.TextureCubeMap);
#else
#endif
                }

                GL.BindTexture(TextureTarget.Texture2D, prevTexture);
                GraphicsExtensions.CheckGLError();


            });
        }


        /// <summary>
        /// 
        /// </summary>
        private void GenerateGLTextureIfRequired()
        {
            if (this.glTexture < 0)
            {
                GL.GenTextures(1, out this.glTexture);
                GraphicsExtensions.CheckGLError();
                var wrap = TextureWrapMode.Repeat;

                GL.BindTexture(TextureTarget.Texture2D, this.glTexture);
                GraphicsExtensions.CheckGLError();
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (_levelCount > 1) ? (int)TextureMinFilter.LinearMipmapLinear : (int)TextureMinFilter.Linear);
                GraphicsExtensions.CheckGLError();
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GraphicsExtensions.CheckGLError();
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrap);
                GraphicsExtensions.CheckGLError();
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrap);
                GraphicsExtensions.CheckGLError();
            }
        }

    }
}
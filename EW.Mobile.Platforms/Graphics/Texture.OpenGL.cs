using System;
#if GLES
using OpenTK.Graphics.ES20;
#endif
namespace EW.Mobile.Platforms.Graphics
{
    public abstract partial class Texture
    {
        internal int glTexture = -1;
        internal TextureTarget glTarget;
        internal TextureUnit glTextureUnit = TextureUnit.Texture0;
        /// <summary>
        /// 纹理储存格式
        /// </summary>
        internal PixelInternalFormat glInternalFormat;

        /// <summary>
        /// 原图格式和数据类型
        /// </summary>
        internal PixelFormat glFormat;
        internal PixelType glType;
        

        private void PlatformGraphicsDeviceResetting()
        {

        }


        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                DeleteGLTexture();

            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// 删除纹理贴图
        /// </summary>
        private void DeleteGLTexture()
        {
            if (glTexture > 0)
            {
                int texture = glTexture;
                Threading.BlockOnUIThread(() => {

                    GL.DeleteTextures(1, ref texture);
                    GraphicsExtensions.CheckGLError();
                });
            }
        }


    }
}
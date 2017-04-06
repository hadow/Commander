using System;
using System.Runtime.InteropServices;
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
        /// <typeparam name="T"></typeparam>
        /// <param name="level">多级渐远纹理的级别</param>
        /// <param name="arraySize"></param>
        /// <param name="rect"></param>
        /// <param name="data">图像数据</param>
        /// <param name="startIndex"></param>
        /// <param name="elementCount"></param>
        private void PlatformSetData<T>(int level,int arraySize,Rectangle? rect,T[] data,int startIndex,int elementCount) where T : struct
        {
            Threading.BlockOnUIThread(() => {

                var elementSizeInByte = Marshal.SizeOf(typeof(T));
                var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    var startBytes = startIndex * elementSizeInByte;
                    var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

                    int x, y, w, h;
                    if (rect.HasValue)
                    {
                        x = rect.Value.X;
                        y = rect.Value.Y;
                        w = rect.Value.Width;
                        h = rect.Value.Height;
                    }
                    else
                    {
                        x = 0;
                        y = 0;
                        w = Math.Max(width >> level, 1);
                        h = Math.Max(height >> level, 1);
                    }

                    var prevTexture = GraphicsExtensions.GetBoundTexture2D();
                    GenerateGLTextureIfRequired();
                    GL.BindTexture(TextureTarget.Texture2D, this.glTexture);
                    GraphicsExtensions.CheckGLError();
                    if(glFormat == (PixelFormat)GLPixelFormat.CompressedTextureFormats)
                    {
                        
                    }
                    else
                    {
                        GL.PixelStore(PixelStoreParameter.UnpackAlignment, GraphicsExtensions.GetSize(this.Format));
                        if (rect.HasValue)
                        {
                            GL.TexSubImage2D(TextureTarget.Texture2D, level, x, y, w, h, glFormat, glType, dataPtr);
                            GraphicsExtensions.CheckGLError();
                        }
                        else
                        {
                            GL.TexImage2D(TextureTarget.Texture2D, level, glInternalFormat, w, h, 0, glFormat, glType, dataPtr);
                            GraphicsExtensions.CheckGLError();
                        }
                        //
                        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
                    }

                    GL.BindTexture(TextureTarget.Texture2D, prevTexture);
                    GraphicsExtensions.CheckGLError();
                    

                }
                finally
                {
                    dataHandle.Free();
                }
                

            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="level"></param>
        /// <param name="arraySlice"></param>
        /// <param name="rect"></param>
        /// <param name="data"></param>
        /// <param name="startIndex"></param>
        /// <param name="elementCount"></param>
        private void PlatformGetData<T>(int level,int arraySlice,Rectangle? rect,T[] data,int startIndex,int elementCount) where T : struct
        {
#if GLES

            var framebufferId = 0;
#if (IOS || ANDROID)
            GL.GenFramebuffers(1, out framebufferId);
#else
            Gl.GenFramebuffers(1,ref framebufferId);
#endif
            GraphicsExtensions.CheckGLError();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);
            GraphicsExtensions.CheckGLError();
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, TextureTarget.Texture2D, this.glTexture, 0);
            GraphicsExtensions.CheckGLError();
            var x = 0;
            var y = 0;
            var width = this.width;
            var height = this.height;
            if (rect.HasValue)
            {
                x = rect.Value.X;
                y = rect.Value.Y;
                width = this.Width;
                height = this.Height;
            }

            GL.ReadPixels(x, y, width, height, this.glFormat, this.glType, data);
            GraphicsExtensions.CheckGLError();
            GL.DeleteFramebuffers(1, ref framebufferId);
            GraphicsExtensions.CheckGLError();

#endif
        }



        /// <summary>
        /// 创建一个帧缓冲的纹理
        /// </summary>
        private void GenerateGLTextureIfRequired()
        {
            if (this.glTexture < 0)
            {
                GL.GenTextures(1, out this.glTexture);
                GraphicsExtensions.CheckGLError();
                var wrap = TextureWrapMode.Repeat;
                if ((width & (width - 1)) != 0 || ((height & (height - 1)) != 0))
                    wrap = TextureWrapMode.ClampToEdge;
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
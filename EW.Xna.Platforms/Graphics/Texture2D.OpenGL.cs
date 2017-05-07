using System;
using System.Runtime.InteropServices;
using System.IO;
#if ANDROID
using Android.Graphics;
#endif
#if GLES
using OpenTK.Graphics.ES20;
using GLPixelFormat = OpenTK.Graphics.ES20.All;
using PixelFormat = OpenTK.Graphics.ES20.PixelFormat;
using PixelInternalFormat = OpenTK.Graphics.ES20.PixelFormat;
#endif
namespace EW.Xna.Platforms.Graphics
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

                //Store the current bound texture
                var prevTexture = GraphicsExtensions.GetBoundTexture2D();

                GenerateGLTextureIfRequired();

                format.GetGLFormat(GraphicsDevice, out glInternalFormat, out glFormat, out glType);
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
        private void PlatformSetData<T>(int level,int arraySize,Rectangle rect,T[] data,int startIndex,int elementCount) where T : struct
        {
            Threading.BlockOnUIThread(() => {

                var elementSizeInByte = Marshal.SizeOf(typeof(T));
                var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    var startBytes = startIndex * elementSizeInByte;
                    var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

                    //Store the current bound texture
                    var prevTexture = GraphicsExtensions.GetBoundTexture2D();

                    GenerateGLTextureIfRequired();

                    GL.BindTexture(TextureTarget.Texture2D, this.glTexture);
                    GraphicsExtensions.CheckGLError();

                    if(glFormat == (PixelFormat)GLPixelFormat.CompressedTextureFormats)
                    {
                        
                    }
                    else
                    {
                        GL.TexSubImage2D(TextureTarget.Texture2D, level, rect.X, rect.Y, rect.Width, rect.Height, glFormat, glType, dataPtr);
                        GraphicsExtensions.CheckGLError();
                    }

                    //Restore the bound texture.
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static Texture2D FromStream(GraphicsDevice graphicsDevice,Stream stream)
        {
            if (graphicsDevice == null)
                throw new ArgumentException("graphicsDevice");
            if (stream == null)
                throw new ArgumentNullException("stream");

            try
            {
                return PlatformFromStream(graphicsDevice, stream);
            }
            catch(Exception e)
            {
                throw new InvalidOperationException("This image format is not supported", e);
            }

        }

        private static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice,Stream stream)
        {
#if ANDROID
            using (Bitmap image = BitmapFactory.DecodeStream(stream,null,new BitmapFactory.Options {

                InScaled = false,
                InDither = false,
                InJustDecodeBounds = false,
                InPurgeable = true,
                InInputShareable = true,

            }))
            {
                return PlatformFromStream(graphicsDevice, image);
            }
#endif
#if DESKTOPGL

#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        private static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice,Bitmap image)
        {
            var width = image.Width;
            var height = image.Height;

            int[] pixels = new int[width * height];

            if((width!=image.Width) || (height != image.Height))
            {
                //TODO


            }
            else
            {
                image.GetPixels(pixels, 0, width, 0, 0, width, height);
            }
            image.Recycle();

            //Convert from ARGB to ABGR
            ConvertToABGR(height, width, pixels);

            Texture2D texture = null;
            Threading.BlockOnUIThread(()=> {

                texture = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Color);
                texture.SetData<int>(pixels);
            });

            return texture;
        }

        /// <summary>
        /// ARGB => ABGR
        /// </summary>
        /// <param name="pixelHeight"></param>
        /// <param name="pixelWidth"></param>
        /// <param name="pixels"></param>
        private static void ConvertToABGR(int pixelHeight,int pixelWidth,int[] pixels)
        {
            int pixelCount = pixelWidth * pixelHeight;
            for(int i = 0; i < pixelCount; i++)
            {
                uint pixel = (uint)pixels[i];
                pixels[i] = (int)((pixel & 0xFF00FF00) | ((pixel & 0x00FF0000) >> 16) | ((pixel & 0x000000FF) << 16));
            }
        }

        

    }
}
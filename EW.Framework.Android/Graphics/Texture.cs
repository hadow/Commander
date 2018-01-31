using System;
using System.Drawing;
using System.IO;
namespace EW.Framework.Graphics
{
    sealed class Texture:GraphicsResource,ITexture
    {
        int texture;
        TextureScaleFilter scaleFilter;

        public Size Size { get; private set; }


        public int ID { get { return texture; } }

        bool disposed;


        public Texture()
        {
            GL.GenTextures(1, out texture);
            GraphicsExtensions.CheckGLError();
        }


        public TextureScaleFilter ScaleFilter
        {
            get { return scaleFilter; }
            set
            {
                Threading.EnsureUIThread();
                if (scaleFilter == value)
                    return;

                scaleFilter = value;
                PrepareTexture();

            }
        }

        public void SetEmpty(int width,int height)
        {
            Threading.EnsureUIThread();
            if (!Exts.IsPowerOf2(width) || !Exts.IsPowerOf2(height))
                throw new InvalidDataException(string.Format("Non-power-of-two array {0}x{1}", width, height));

            Size = new Size(width, height);
            PrepareTexture();
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.BGRA_EXT, PixelType.UnsignedByte, IntPtr.Zero);
            //GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GraphicsExtensions.CheckGLError();
        }


        public void SetData(byte[] colors,int width,int height)
        {
            Threading.EnsureUIThread();
            if (!Exts.IsPowerOf2(width) || !Exts.IsPowerOf2(height))
                throw new InvalidDataException(string.Format("Non-power-of-two array {0} X {1}", width, height));

            Size = new Size(width, height);
            //ConvertToABGR(height, width, colors);
            unsafe
            {
                fixed(byte*ptr = &colors[0])
                {
                    var intPtr = new IntPtr((void*)ptr);
                    PrepareTexture();
                    //GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, intPtr);
                    GL.TexImage2D(TextureTarget.Texture2D, 0,
                                  PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.BGRA_EXT, PixelType.UnsignedByte, intPtr);
                    //GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, PixelFormat.BGRA_EXT, PixelType.UnsignedByte, intPtr);
                    GraphicsExtensions.CheckGLError();
                }
            }
        }


        /// <summary>
        /// Convert from ARGB to ABGR
        /// </summary>
        /// <param name="pixelHeight"></param>
        /// <param name="pixelWidth"></param>
        /// <param name="colors"></param>
        //private static void ConvertToABGR(int pixelHeight, int pixelWidth, byte[] colors)
        //{
        //    int pixelCount = pixelHeight * pixelWidth;
        //    //int pixelCount = colors.Length;
        //    for(int i = 0; i < pixelCount; i++)
        //    {
        //        uint pixel = colors[i];
        //        colors[i] = (byte)((pixel & 0xFF00FF00) | ((pixel & 0x00FF0000) >> 16) | ((pixel & 0x000000FF) << 16));
        //    }
        //}

        //public static void ConvertTARGB(byte[] colors) {

        //    for (int i = 0; i < colors.Length; i++) {
        //        uint pixel = colors[i];
        //        colors[i] = (byte)((pixel & 0xFFFFFF00) | ((pixel&0x000000FF)<<24));
        //    }
        //}


        //An array of RGBA
        public void SetData(uint[,] colors)
        {
            Threading.EnsureUIThread();

            var width = colors.GetUpperBound(1) + 1;
            var height = colors.GetUpperBound(0) + 1;

            if (!Exts.IsPowerOf2(width) || !Exts.IsPowerOf2(height))
                throw new InvalidDataException(string.Format("Non-power-of-two array {0}x{1}", width, height));
            
            Size = new Size(width, height);

            unsafe
            {
                fixed (uint* ptr = &colors[0, 0])
                {
                    var intPtr = new IntPtr((void*)ptr);
                    PrepareTexture();
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
                        width, height, 0, PixelFormat.BGRA_EXT, PixelType.UnsignedByte, intPtr);

                }
            }
        }

        

        void PrepareTexture()
        {
            GraphicsExtensions.CheckGLError();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GraphicsExtensions.CheckGLError();

            var filter = scaleFilter == TextureScaleFilter.Linear ? TextureMinFilter.Linear : TextureMinFilter.Nearest;

            GL.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)filter);

            GraphicsExtensions.CheckGLError();
            GL.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)filter);
            GraphicsExtensions.CheckGLError();

            GL.TexParameterf(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            GraphicsExtensions.CheckGLError();

            GL.TexParameterf(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
            GraphicsExtensions.CheckGLError();

            //GL.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            //GraphicsExtensions.CheckGLError();

            //GL.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);
            //GraphicsExtensions.CheckGLError();

            //if (GraphicsDevice.GraphicsCapabilities.SupportsTextureMaxLevel)
            //{
            //    GL.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 1000);
            //    GraphicsExtensions.CheckGLError();
            //}
        }


        


        public byte[] GetData()
        {
            Threading.EnsureUIThread();
            var data = new byte[4 * Size.Width * Size.Height];

            GraphicsExtensions.CheckGLError();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            unsafe
            {
                fixed(byte*ptr = &data[0])
                {
                    var intPtr = new IntPtr((void*)ptr);

                    GL.GetTexImageInternal(TextureTarget.Texture2D, 0, PixelFormat.BGRA_EXT, PixelType.UnsignedByte, intPtr);
                }
            }

            
            GraphicsExtensions.CheckGLError();
            return data;
        }
        

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposed)
                return;
            disposed = true;
            GL.DeleteTextures(1, ref texture);
        }



    }
}
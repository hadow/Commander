using System;


namespace EW.Mobile.Platforms.Graphics
{
    public partial class Texture2D:Texture
    {
        internal protected enum SurfaceType
        {
            Texture,
            RenderTarget,
            SwapChainRenderTarget,
        }

        internal int width;
        internal int height;
        internal int arraySize;

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mipmap"></param>
        /// <param name="format"></param>
        /// <param name="type"></param>
        /// <param name="shared"></param>
        /// <param name="arraySize"></param>
        protected Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared, int arraySize)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width", "Texture width must be greater than zero ");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height", "Texture height must be greater than zero ");

            this.GraphicsDevice = graphicsDevice;
            this.width = width;
            this.height = height;
            this.arraySize = arraySize;
            this._format = format;
            this._levelCount = mipmap ? CalculateMipLevels(width, height) : 1;

            if (type == SurfaceType.SwapChainRenderTarget)
                return;
            PlatformConstruct(width, height, mipmap, format, type, shared);
        }

        public Texture2D(GraphicsDevice graphicsDevice,int width,int height) : this(graphicsDevice, width, height, false, SurfaceFormat.Color, SurfaceType.Texture, false, 1){ }

        public Texture2D(GraphicsDevice graphicsDevice,int width,int height,bool mipmap,SurfaceFormat format) : this(graphicsDevice, width, height, mipmap, format, SurfaceType.Texture, false, 1) { }

        public Texture2D(GraphicsDevice graphicsDevice,int width,int height,bool mipmap,SurfaceFormat format,int arraySize) : this(graphicsDevice, width, height, mipmap, format, SurfaceType.Texture, false, arraySize) { }

        internal Texture2D(GraphicsDevice graphicsDevice,int width,int height,bool mipmap,SurfaceFormat format,SurfaceType type) : this(graphicsDevice, width, height, mipmap, format, type, false, 1) { }



    }
}
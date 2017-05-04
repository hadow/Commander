using System;
namespace EW.Xna.Platforms.Graphics
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
        /// 纹理尺寸
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(0, 0, this.width, this.height);
            }
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


        /// <summary>
        /// 获取纹理数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public void GetData<T>(T[] data) where T :struct
        {
            this.GetData(0, null, data, 0, data.Length);
        }

        public void GetData<T>(int level,Rectangle? rect,T[] data,int startIndex,int elementCount) where T : struct
        {
            this.GetData(level, 0, rect, data, startIndex, elementCount);
        }

        public void GetData<T>(int level,int arraySlice,Rectangle? rect,T[] data,int startIndex,int elementCount) where T : struct
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("data cannot be null");
            if (data.Length < startIndex + elementCount)
                throw new ArgumentException("The data passed has a length of " + data.Length + " but" + elementCount + "  pixels have been requested.");
            if (arraySlice > 0 && !GraphicsDevice.GraphicsCapabilities.SupportsTextrueArrays)
                throw new ArgumentException("Texture arrays are not supported on this graphics device", "arraySlice");
            if (rect.HasValue && rect.Value.Width * rect.Value.Height != elementCount)
                throw new ArgumentException("The size of the data passed in is too large or too small for this recources");
            PlatformGetData<T>(level, arraySlice, rect, data, startIndex, elementCount);

        }

        /// <summary>
        /// Changes the texture's pixels
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public void SetData<T>(T[] data) where T : struct
        {
            this.SetData(0, null, data, 0, data.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="level"></param>
        /// <param name="rect"></param>
        /// <param name="data"></param>
        /// <param name="startIndex"></param>
        /// <param name="elementCount"></param>
        public void SetData<T>(int level,Rectangle? rect,T[] data,int startIndex,int elementCount) where T : struct
        {
            this.SetData(level, 0, rect, data, startIndex, elementCount);
        }

        public void SetData<T>(int level,int arraySlice,Rectangle? rect,T[] data,int startIndex,int elementCount) where T : struct
        {
            Rectangle resizedBounds = new Rectangle(0, 0, Math.Max(Bounds.Width >> level, 1), Math.Max(Bounds.Height >> level, 1));

            if (level >= LevelCount)
                throw new ArgumentException("Texture only has " + _levelCount + "levels", "level");
            if (data == null || data.Length == 0)
                throw new ArgumentException("data cannot be null");
            if((!rect.HasValue &&(data.Length-startIndex<resizedBounds.Width * resizedBounds.Height)) || (rect.HasValue && (rect.Value.Height * rect.Value.Width > data.Length)))
            {
                throw new ArgumentException("data array is to small");
            }

            if (elementCount + startIndex > data.Length)
                throw new ArgumentException("ElementCount must be a valid index in the data array", "elementCount");
            if (arraySlice > 0 && !GraphicsDevice.GraphicsCapabilities.SupportsTextrueArrays)
                throw new ArgumentException("Texture arrays are not supported on this graphics device", "arraySlice");
            if (arraySlice >= this.arraySize)
                throw new ArgumentException("Texture array only has " + arraySize + " textures", "arraySlice");
            if (rect.HasValue && !resizedBounds.Contains(rect.Value))
                throw new ArgumentException("Rectangle must be inside the Texture Bounds", "rect");
            PlatformSetData<T>(level, arraySlice, resizedBounds, data, startIndex, elementCount);

        }
    }
}
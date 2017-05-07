using System;
using EW.Xna.Platforms.Utilities;
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
        /// <typeparam name="T">New Data for the texture</typeparam>
        /// <param name="data"></param>
        public void SetData<T>(T[] data) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data");
            this.SetData(0, null, data, 0, data.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="level">Layer of the texture to modify</param>
        /// <param name="rect">Area to modify</param>
        /// <param name="data"></param>
        /// <param name="startIndex">Start position of data</param>
        /// <param name="elementCount"></param>
        public void SetData<T>(int level,Rectangle? rect,T[] data,int startIndex,int elementCount) where T : struct
        {
            this.SetData(level, 0, rect, data, startIndex, elementCount);
        }

        public void SetData<T>(int level,int arraySlice,Rectangle? rect,T[] data,int startIndex,int elementCount) where T : struct
        {
            Rectangle checkedRect;
            ValidateParams(level, arraySlice, rect, data, startIndex, elementCount, out checkedRect);
            PlatformSetData<T>(level, arraySlice, checkedRect, data, startIndex, elementCount);

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
        /// <param name="checkedRect"></param>
        private void ValidateParams<T>(int level,int arraySlice,Rectangle? rect,T[] data,int startIndex,int elementCount,out Rectangle checkedRect) where T : struct
        {
            var textureBounds = new Rectangle(0, 0, Math.Max(width >> level, 1), Math.Max(height >> level, 1));
            checkedRect = rect ?? textureBounds;


            if (level < 0 || level >= LevelCount)
                throw new ArgumentException("level must be samller than the number of levels in this texture");

            if (arraySlice > 0 && !GraphicsDevice.GraphicsCapabilities.SupportsTextrueArrays)
                throw new ArgumentException("Texture arrays are not supported on this graphics device", "arraySlice");

            if (arraySlice < 0 || arraySlice >= arraySize)
                throw new ArgumentException("arraySlice must be smaller than the arraySize of this texture and larger than 0.");

            if (!textureBounds.Contains(checkedRect) || checkedRect.Width <= 0 || checkedRect.Height <= 0)
                throw new ArgumentException("Rectangle must be inside the texture bounds", "rect");

            if (data == null)
                throw new ArgumentNullException("data");

            var tSize = ReflectionHelpers.SizeOf<T>.Get();
            var fSize = Format.GetSize();
            if (tSize > fSize || fSize % tSize != 0)
                throw new ArgumentException("Type T is of an invalid size for the format of this texture.", "T");

            if (startIndex < 0 || startIndex >= data.Length)
                throw new ArgumentException("startIndex must be at least zero and smaller than data.lenth.", "startIndex");

            if (data.Length < startIndex + elementCount)
                throw new ArgumentException("The data array is to small.");

            int dataByteSize = 0;
            if (Format.IsCompressedFormat())
            {

            }
            else
            {
                dataByteSize = checkedRect.Width * checkedRect.Height * fSize;
            }
            if (elementCount * tSize != dataByteSize)
                throw new ArgumentException(string.Format("elementCount is not the right size, elementCount * sizeof(T) is {0},but data size is {1}.", elementCount * tSize, dataByteSize), "elementCount");

        }
    }
}
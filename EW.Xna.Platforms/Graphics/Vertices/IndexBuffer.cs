using System;

namespace EW.Xna.Platforms.Graphics
{
    public enum SetDataOptions
    {
        None,
        Discard,
        NoOverwrite,
    }
    public enum IndexElementSize
    {
        SixteenBits,
        ThirtyTwoBits,
    }

    public enum BufferUsage
    {
        None,
        WriteOnly,
    }
    /// <summary>
    /// 
    /// </summary>
    public partial class IndexBuffer:GraphicsResource
    {
        private readonly bool _isDynamic;

        public int IndexCount { get; private set; }

        public BufferUsage BufferUsage { get; private set; }

        public IndexElementSize IndexElementSize { get; private set; }
        protected IndexBuffer(GraphicsDevice graphicsDevice,IndexElementSize indexElementSize,
            int indexCount,BufferUsage usage,bool dynamic)
        {


        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="offsetInBytes"></param>
        /// <param name="data"></param>
        /// <param name="startIndex"></param>
        /// <param name="elementCount"></param>
        /// <param name="options"></param>
        protected void SetDataInternal<T>(int offsetInBytes,T[] data,int startIndex,int elementCount,SetDataOptions options) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (data.Length < (startIndex + elementCount))
                throw new InvalidOperationException("The array specified in the data parameter is not the correct size for the amount of data requested.");

            PlatformSetDataInternal(offsetInBytes, data, startIndex, elementCount, options);
        }

    }
}
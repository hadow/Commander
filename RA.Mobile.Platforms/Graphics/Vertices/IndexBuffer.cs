using System;

namespace RA.Mobile.Platforms.Graphics
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

    }
}
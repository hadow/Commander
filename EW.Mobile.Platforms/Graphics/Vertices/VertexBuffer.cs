using System;

namespace EW.Mobile.Platforms.Graphics
{
    /// <summary>
    /// 顶点缓冲对象管理顶点数据
    /// </summary>
    public partial class VertexBuffer:GraphicsResource
    {

        private readonly bool _isDynamic;

        public int VertexCount { get; private set; }
        //声明的顶点数据
        public VertexDeclaration VertexDeclaration { get; private set; }


        public BufferUsage BufferUsage { get; private set; }


        protected VertexBuffer(GraphicsDevice graphicsDevice,VertexDeclaration vertexDeclaration,int vertexCount,BufferUsage bufferUsage,bool dynamic)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");

            this.GraphicsDevice = graphicsDevice;
            this.VertexDeclaration = vertexDeclaration;
            this.VertexCount = vertexCount;
            this.BufferUsage = bufferUsage;

            if (vertexDeclaration.GraphicsDevice != graphicsDevice)
                vertexDeclaration.GraphicsDevice = graphicsDevice;

            _isDynamic = dynamic;
            PlatformConstruct();
        }

        public VertexBuffer(GraphicsDevice graphicsDevice,VertexDeclaration vertexDeclaration,int vertexCount,BufferUsage bufferUsage) : this(graphicsDevice, vertexDeclaration, vertexCount, bufferUsage, false)
        {

        }

        protected internal override void GraphicsDeviceResetting()
        {
            PlatformGraphicsDeviceResetting();
        }


        public void SetData<T>(T[] data,int startIndex,int elementCount) where T : struct
        {
            var elementSizeInBytes = Utilities.ReflectionHelpers.SizeOf<T>.Get();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="offsetInBytes"></param>
        /// <param name="data"></param>
        /// <param name="startIndex"></param>
        /// <param name="elementCount"></param>
        /// <param name="vertexStride"></param>
        /// <param name="options"></param>
        protected void SetDataInternal<T>(int offsetInBytes,T[] data,int startIndex,int elementCount,int vertexStride,SetDataOptions options) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data");

            var elementSizeInBytes = Utilities.ReflectionHelpers.SizeOf<T>.Get();
            var bufferSize = VertexCount * VertexDeclaration.VertexStride;

            if (vertexStride == 0)
                vertexStride = elementSizeInBytes;

            if (startIndex + elementCount > data.Length || elementCount <= 0)
                throw new ArgumentOutOfRangeException("data", "The array specified in the data parameter is not the correct size for the amount of data requested.");

            if (elementCount > 1 && (elementCount * vertexStride > bufferSize))
                throw new InvalidOperationException("The vertex stride is larger than the vertex buffer.");
            if (vertexStride < elementSizeInBytes)
                throw new ArgumentOutOfRangeException(string.Format("The vertex stride must be greater than or equal to the size of the specified data ({0}).", elementSizeInBytes));


        }

    }
}
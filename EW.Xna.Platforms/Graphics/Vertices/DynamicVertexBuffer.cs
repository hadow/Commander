using System;

namespace EW.Xna.Platforms.Graphics
{
    public class DynamicVertexBuffer:VertexBuffer
    {

        public DynamicVertexBuffer(GraphicsDevice graphicsDevice,VertexDeclaration vertexDeclaration,int vertexCount,BufferUsage bufferUsage) : base(graphicsDevice, vertexDeclaration, vertexCount, bufferUsage, true) { }        

        public DynamicVertexBuffer(GraphicsDevice graphicsDevice,Type type,int vertexCount,BufferUsage bufferUsage) : base(graphicsDevice, VertexDeclaration.FromT(type), vertexCount, bufferUsage, true) { }

        public void SetData<T>(int offsetInBytes,T[] data,int startIndex,int elementCount,int vertexStride,SetDataOptions options) where T : struct
        {
            base.SetDataInternal<T>(offsetInBytes, data, startIndex, elementCount, vertexStride, options);
        }

        public void SetData<T>(T[] data,int startIndex,int elementCount,SetDataOptions options) where T : struct
        {
            var elementSizeInBytes = Utilities.ReflectionHelpers.SizeOf<T>.Get();
            base.SetDataInternal<T>(0, data, startIndex, elementCount, elementSizeInBytes, options);
        }

        public void SetData(IntPtr data,int start,int length)
        {

        }


    }
}
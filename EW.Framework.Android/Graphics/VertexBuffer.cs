using System;
using System.Runtime.InteropServices;

namespace EW.Framework.Graphics
{
    sealed class VertexBuffer<T>:IVertexBuffer<T> where T:struct
    {

        static readonly int VertexSize = Marshal.SizeOf(typeof(T));

        int buffer;
        bool disposed;

        public VertexBuffer(int size)
        {
            GL.GenBuffers(1, out buffer);
            GraphicsExtensions.CheckGLError();
            Bind();

            //Generates a buffer with uninitialized memory.
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(VertexSize * size), IntPtr.Zero, BufferUsageHint.DynamicDraw);

            GraphicsExtensions.CheckGLError();
            //We need to zero all the memory,Let's generate a smallish array and copy that over the whole buffer.
            var zeroedArrayElementSize = Math.Min(size, 2048);

            var ptr = GCHandle.Alloc(new T[zeroedArrayElementSize], GCHandleType.Pinned);
            try
            {
                for(var offset = 0; offset < size; offset += zeroedArrayElementSize)
                {
                    var length = Math.Min(zeroedArrayElementSize, size - offset);
                    GL.BufferSubData(BufferTarget.ArrayBuffer,
                        new IntPtr(VertexSize * offset),
                        new IntPtr(VertexSize * length),
                        ptr.AddrOfPinnedObject());
                    GraphicsExtensions.CheckGLError();
                }

                //GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(VertexSize * size), ptr.AddrOfPinnedObject(), BufferUsageHint.DynamicDraw);
            }
            finally
            {
                ptr.Free();
            }

            //GraphicsExtensions.CheckGLError();
        }


        ~VertexBuffer()
        {
            Dispose(false);
        }

        public void Bind()
        {
            Threading.EnsureUIThread();
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
            GraphicsExtensions.CheckGLError();
            GL.VertexAttribPointer(Shader.VertexPosAttributeIndex, 3, VertexAttribPointerType.Float, false, VertexSize, IntPtr.Zero);
            GraphicsExtensions.CheckGLError();
            GL.VertexAttribPointer(Shader.TexCoordAttributeIndex, 4, VertexAttribPointerType.Float, false, VertexSize, new IntPtr(12));
            GraphicsExtensions.CheckGLError();
            GL.VertexAttribPointer(Shader.TexMetadataAttributeIndex, 2, VertexAttribPointerType.Float, false, VertexSize, new IntPtr(28));
            GraphicsExtensions.CheckGLError();

        }


        public void SetData(T[] data,int length)
        {
            SetData(data, 0, length);
        }


        public void SetData(T[] data,int start,int length)
        {
            Bind();

            var ptr = GCHandle.Alloc(data, GCHandleType.Pinned);

            try
            {
                GL.BufferSubData(BufferTarget.ArrayBuffer,
                    new IntPtr(VertexSize * start),
                    new IntPtr(VertexSize * length),
                    ptr.AddrOfPinnedObject());
            }
            finally
            {
                ptr.Free();
            }

            GraphicsExtensions.CheckGLError();
        }


        public void SetData(IntPtr data,int start,int length)
        {
            Bind();

            GL.BufferSubData(BufferTarget.ArrayBuffer, new IntPtr(VertexSize * start), new IntPtr(VertexSize*length), data);
            GraphicsExtensions.CheckGLError();
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposed)
                return;
            disposed = true;
            GL.DeleteBuffers(1, ref buffer);
        }

    }
}
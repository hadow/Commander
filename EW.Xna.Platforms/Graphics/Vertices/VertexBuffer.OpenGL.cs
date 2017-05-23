using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#if GLES
using OpenTK.Graphics.ES20;
using BufferUsageHint = OpenTK.Graphics.ES20.BufferUsage;
#endif


namespace EW.Xna.Platforms.Graphics
{
    public partial class VertexBuffer
    {
        //internal uint vbo
        internal uint vbo;

        private void PlatformConstruct()
        {
            Threading.BlockOnUIThread(GenerateIfRequired);
        }


        /// <summary>
        /// 生成顶点缓冲对象
        /// </summary>
        void GenerateIfRequired()
        {
            if(vbo == 0)
            {
                GL.GenBuffers(1, out this.vbo);//
                GraphicsExtensions.CheckGLError();
                //bind to the buffer,Future commands will affect this buffer specifically
                GL.BindBuffer(BufferTarget.ArrayBuffer, this.vbo);
                GraphicsExtensions.CheckGLError();
                //定义的顶点数据复制到缓冲的内存中              
                //parameter1:目标缓冲的类型
                //parameter2:传输数据的大小(以字节为单位)                           
                //parameter3:希望发送的实际数据 
                //parameter4:显卡如何管理给定的数据    (StreamDraw:数据每次绘制都会改变 staticDraw:数据不会或几乎不会改变)
                GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(VertexDeclaration.VertexStride * VertexCount), IntPtr.Zero, _isDynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);

                GraphicsExtensions.CheckGLError();
            }
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
        /// <param name="bufferSize"></param>
        /// <param name="elementSizeInBytes"></param>
        private void PlatformSetDataInternal<T>(int offsetInBytes,T[] data,int startIndex,int elementCount,int vertexStride,SetDataOptions options,int bufferSize,int elementSizeInBytes) where T : struct
        {
            if (Threading.IsOnUIThread())
            {
                SetBufferData(bufferSize, elementSizeInBytes, offsetInBytes, data, startIndex, elementCount, vertexStride, options);
            }
            else
            {
                Threading.BlockOnUIThread(() =>
                {
                    SetBufferData(bufferSize, elementSizeInBytes, offsetInBytes, data, startIndex, elementCount, vertexStride, options);
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bufferSize"></param>
        /// <param name="elementSizeInBytes"></param>
        /// <param name="offsetInBytes"></param>
        /// <param name="data"></param>
        /// <param name="startIndex"></param>
        /// <param name="elementCount"></param>
        /// <param name="vertexStride"></param>
        /// <param name="options"></param>
        private void SetBufferData<T>(int bufferSize,int elementSizeInBytes,int offsetInBytes,T[] data,int startIndex,int elementCount,int vertexStride,SetDataOptions options) where T : struct
        {
            GenerateIfRequired();

            var sizeInBytes = elementSizeInBytes * elementCount;
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GraphicsExtensions.CheckGLError();

            if(options == SetDataOptions.Discard)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)bufferSize, IntPtr.Zero, _isDynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
                GraphicsExtensions.CheckGLError();
            }

            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInBytes);

            int dataSize = Marshal.SizeOf(typeof(T));
            if(dataSize == vertexStride)
            {
                GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)offsetInBytes, (IntPtr)sizeInBytes, dataPtr);
                GraphicsExtensions.CheckGLError();
            }
            else
            {
                for(int i = 0; i < elementCount; i++)
                {
                    GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)offsetInBytes + i * vertexStride, (IntPtr)dataSize, dataPtr);
                    GraphicsExtensions.CheckGLError();
                    dataPtr = (IntPtr)(dataPtr.ToInt64() + dataSize);
                }
            }

            dataHandle.Free();
        }

        private void PlatformGraphicsDeviceResetting()
        {
            vbo = 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                Threading.BlockOnUIThread(() =>
                {
                    ///When we no longer want to keep our buffers around,we can free the memory;
                    GL.DeleteBuffers(1, ref vbo);
                    GraphicsExtensions.CheckGLError();
                });
            }

            base.Dispose(disposing);
        }


    }
}
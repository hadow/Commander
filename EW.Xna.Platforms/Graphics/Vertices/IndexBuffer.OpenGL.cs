using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.ES20;
namespace EW.Xna.Platforms.Graphics
{

    public partial class IndexBuffer
    {
        internal uint ibo;


        void GenerateIfRequired()
        {
            if(ibo == 0)
            {
                var sizeInBytes = IndexCount * (this.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);
                GL.GenBuffers(1, out ibo);
                GraphicsExtensions.CheckGLError();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
                GraphicsExtensions.CheckGLError();
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)sizeInBytes, IntPtr.Zero, _isDynamic ? OpenTK.Graphics.ES20.BufferUsage.StreamDraw : OpenTK.Graphics.ES20.BufferUsage.StreamDraw);
                GraphicsExtensions.CheckGLError();
            }
        }


        private void PlatformSetDataInternal<T>(int offsetInBytes,T[] data,int startIndex,int elementCount,SetDataOptions options) where T : struct
        {
            if (Threading.IsOnUIThread())
            {
                BufferData(offsetInBytes, data, startIndex, elementCount, options);
            }
            else
            {
                Threading.BlockOnUIThread(() => BufferData(offsetInBytes, data, startIndex, elementCount, options));
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
        /// <param name="options"></param>
        private void BufferData<T>(int offsetInBytes,T[] data,int startIndex,int elementCount,SetDataOptions options) where T : struct
        {
            GenerateIfRequired();

            var elementSizeInByte = Marshal.SizeOf(typeof(T));
            var sizeInBytes = elementSizeInByte * elementCount;
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);
            var bufferSize = IndexCount * (IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GraphicsExtensions.CheckGLError();

            if(options == SetDataOptions.Discard)
            {
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)bufferSize, IntPtr.Zero, _isDynamic ? OpenTK.Graphics.ES20.BufferUsage.StreamDraw : OpenTK.Graphics.ES20.BufferUsage.StaticDraw);
                GraphicsExtensions.CheckGLError();
            }
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)offsetInBytes, (IntPtr)sizeInBytes, dataPtr);
            GraphicsExtensions.CheckGLError();

            dataHandle.Free();
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                Threading.BlockOnUIThread(()=> {

                    GL.DeleteBuffers(1, ref ibo);
                    GraphicsExtensions.CheckGLError();

                });
            }


            base.Dispose(disposing);
        }

    }
}
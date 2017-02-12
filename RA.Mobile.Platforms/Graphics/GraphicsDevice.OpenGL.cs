using System;
using System.Collections.Generic;
using OpenTK.Graphics.ES20;
using GLPrimitiveType= OpenTK.Graphics.ES20.BeginMode;
namespace RA.Mobile.Platforms.Graphics
{
    /// <summary>
    /// Graphics Device For OPEN GL
    /// </summary>
    public partial class GraphicsDevice
    {

        static List<Action> disposeActions = new List<Action>();
        static object disposeActionsLock = new object();

        internal int glMajorVersion = 0;
        internal int glMinorVersion = 0;

        internal static readonly List<int> _enabledVertexAttributes = new List<int>();//已启用顶点属性
        private static GLPrimitiveType PrimitiveTypeGL(PrimitiveType primitiveType)
        {
            switch(primitiveType)
            {
                case PrimitiveType.LineList:
                    return GLPrimitiveType.Lines;
                case PrimitiveType.LineStrip:
                    return GLPrimitiveType.LineStrip;
                case PrimitiveType.TriangleList:
                    return GLPrimitiveType.Triangles;
                case PrimitiveType.TriangleStrip:
                    return GLPrimitiveType.TriangleStrip;
            }

            throw new ArgumentException();
        }



        /// <summary>
        /// 平台初始化
        /// </summary>
        private void PlatformInitialize()
        {
            _viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void PlatformSetViewport(ref Viewport value)
        {
            if (IsRenderTargetBound)
                GL.Viewport(value.X, value.Y, value.Width, value.Height);
            else
                GL.Viewport(value.X, PresentationParameters.BackBufferHeight - value.Y - value.Height, value.Width, value.Height);
            GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.Viewport");

#if GLES
            GL.DepthRange(value.MinDepth, value.MaxDepth);
#endif
            GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.DepthRange");

            _vertexShaderDirty = true;

        }

        /// <summary>
        /// 
        /// </summary>
        public void PlatformPresent()
        {
            GraphicsExtensions.CheckGLError();
            lock (disposeActionsLock)
            {
                if (disposeActions.Count > 0)
                {
                    foreach (var action in disposeActions)
                        action();
                    disposeActions.Clear();
                            
                }
            }
        }

    }
}
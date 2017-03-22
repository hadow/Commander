using System;
using System.Collections.Generic;
using OpenTK.Graphics.ES20;
using Android.Util;
namespace EW.Mobile.Platforms.Graphics
{
    static class GraphicsExtensions
    {
#if OPENGL
        public static int OpenGLNumberOfElements(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return 1;
                case VertexElementFormat.Vector2:
                    return 2;
                case VertexElementFormat.Vector3:
                    return 3;
                case VertexElementFormat.Vector4:
                    return 4;
                case VertexElementFormat.Byte4:
                    return 4;
            }

            throw new ArgumentException();
        }

        public static VertexAttribPointerType OpenGLVertexAttribPointerT(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                case VertexElementFormat.Vector2:
                case VertexElementFormat.Vector3:
                case VertexElementFormat.Vector4:
                    return VertexAttribPointerType.Float;

            }

            throw new ArgumentException();
        }


        public static bool OpenGLVertexAttribNormalized(this VertexElement element)
        {
            if (element.VertexElementUsage == VertexElementUsage.Color)
                return true;

            switch (element.VertexElementFormat)
            {
                case VertexElementFormat.NormalizedShort2:
                case VertexElementFormat.NormalizedShort4:
                    return true;
                default:
                    return false;
            }
        }
#endif
        /// <summary>
        /// 
        /// </summary>
        public static void CheckGLError()
        {
#if GLES
            var error = GL.GetErrorCode();

#endif
            if (error != ErrorCode.NoError)
                throw new RAGameGLException("GL.GetError() returned" + error);
        }
#if OPENGL

        public static int GetBoundTexture2D()
        {
            var prevTexture = 0;
            GL.GetInteger(GetPName.TextureBinding2D, out prevTexture);
            return prevTexture;
        }


#endif

#if OPENGL
        public static void LogGLError(string location)
        {
            try
            {
                CheckGLError();
            }
            catch(RAGameGLException ex)
            {
#if ANDROID
                Log.Debug("RA Game", "RAGameGLException at" + location + "-" + ex.Message);
#endif
            }
        }
#endif

        public static int GetSize(this SurfaceFormat surfaceFormat)
        {
            switch (surfaceFormat)
            {
                case SurfaceFormat.Dxt1:
                    return 8;
                case SurfaceFormat.DXt3:
                    return 16;
                default:
                    throw new ArgumentException();

            }
        }

    }

    internal class RAGameGLException : Exception
    {
        public RAGameGLException(string message) : base(message)
        {

        }
    }
}
using System;
using System.Collections.Generic;
using OpenTK.Graphics.ES20;
using Android.Util;
namespace EW.Xna.Platforms.Graphics
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

        public static int GetSize(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return 4;
                case VertexElementFormat.Vector2:
                    return 8;
                case VertexElementFormat.Vector3:
                    return 12;
                case VertexElementFormat.Vector4:
                    return 16;
                case VertexElementFormat.Color:
                    return 4;
            }
            return 0;
        }

        public static BlendEquationMode GetBlendEquationMode(this BlendFunction function)
        {
            switch (function)
            {
                case BlendFunction.Add:
                    return BlendEquationMode.FuncAdd;
                case BlendFunction.Subtract:
                    return BlendEquationMode.FuncSubtract;
                case BlendFunction.ReverseSubtrace:
                    return BlendEquationMode.FuncReverseSubtract;
                default:
                    throw new ArgumentException();

            }
        }

        public static BlendingFactorSrc GetBlendFactorSrc(this Blend blend)
        {
            switch (blend)
            {
                case Blend.DestinationAlpha:
                    return BlendingFactorSrc.DstAlpha;
                case Blend.DestinationColor:
                    return BlendingFactorSrc.DstColor;
                case Blend.InverseDestinationAlpha:
                    return BlendingFactorSrc.OneMinusDstAlpha;
                case Blend.InverseDestinationColor:
                    return BlendingFactorSrc.OneMinusDstColor;
                case Blend.InverseSourceAlpha:
                    return BlendingFactorSrc.OneMinusSrcAlpha;
                case Blend.InverseSourceColor:
                    return BlendingFactorSrc.OneMinusSrcColor;
                case Blend.One:
                    return BlendingFactorSrc.One;
                case Blend.SourceAlpha:
                    return BlendingFactorSrc.SrcAlpha;
                case Blend.SourceAlphaSaturation:
                    return BlendingFactorSrc.SrcAlphaSaturate;
                case Blend.SourceColor:
                    return BlendingFactorSrc.SrcColor;
                case Blend.Zero:
                    return BlendingFactorSrc.Zero;
                default:
                    return BlendingFactorSrc.One;

            }
        }

        public static BlendingFactorDest GetBlendFactorDest(this Blend blend)
        {
            switch (blend)
            {
                case Blend.DestinationAlpha:
                    return BlendingFactorDest.DstAlpha;
                case Blend.InverseDestinationAlpha:
                    return BlendingFactorDest.OneMinusDstAlpha;
                case Blend.InverseSourceAlpha:
                    return BlendingFactorDest.OneMinusSrcAlpha;
                case Blend.InverseSourceColor:
                    return BlendingFactorDest.OneMinusSrcColor;
                case Blend.One:
                    return BlendingFactorDest.One;
                case Blend.SourceAlpha:
                    return BlendingFactorDest.SrcAlpha;
                case Blend.SourceColor:
                    return BlendingFactorDest.SrcColor;
                case Blend.Zero:
                    return BlendingFactorDest.Zero;
                default:
                    return BlendingFactorDest.One;
            }
        }

        public static DepthFunction GetDepthFunction(this CompareFunction compare)
        {
            switch (compare)
            {
                default:
                case CompareFunction.Always:
                    return DepthFunction.Always;
                case CompareFunction.Equal:
                    return DepthFunction.Equal;
                case CompareFunction.Greater:
                    return DepthFunction.Greater;
                case CompareFunction.GreaterEqual:
                    return DepthFunction.Gequal;
                case CompareFunction.Less:
                    return DepthFunction.Less;
                case CompareFunction.LessEqual:
                    return DepthFunction.Lequal;
                case CompareFunction.Never:
                    return DepthFunction.Never;
                case CompareFunction.NotEqual:
                    return DepthFunction.Notequal;
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
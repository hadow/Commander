using System;
using System.Collections.Generic;
using OpenTK.Graphics.ES20;
using Android.Util;
namespace RA.Mobile.Platforms.Graphics
{
    static class GraphicsExtensions
    {

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
    }

    internal class RAGameGLException : Exception
    {
        public RAGameGLException(string message) : base(message)
        {

        }
    }
}
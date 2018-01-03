using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace EW.Framework.Graphics
{
    public class GraphicsExtensions
    {

        [Conditional("DEBUG")]
        [DebuggerHidden]
        public static void CheckGLError()
        {
            var error = GL.GetError();

            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }

    }
}
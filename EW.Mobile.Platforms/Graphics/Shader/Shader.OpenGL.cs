using System;
using System.Collections.Generic;
using OpenTK.Graphics.ES20;

namespace EW.Mobile.Platforms.Graphics
{

    
    internal partial class Shader
    {
        private string _glslCode;

        //
        private int _shaderHandler = -1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="usage"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        internal int GetAttribLocation(VertexElementUsage usage,int index)
        {
            for (int i =0; i < Attributes.Length; i++)
            {
                if((Attributes[i].usage == usage) && (Attributes[i].index == index))
                {
                    return Attributes[i].location;
                }
            }
            return -1;
        }


        /// <summary>
        /// 重置平台上的图形设备
        /// </summary>
        private void PlatformGraphicsDeviceResetting()
        {
            if(_shaderHandler != -1)
            {
                if (GL.IsShader(_shaderHandler))
                {
                    GL.DeleteShader(_shaderHandler);
                    GraphicsExtensions.CheckGLError();
                }
                _shaderHandler = -1;
            }
        }



    }
}
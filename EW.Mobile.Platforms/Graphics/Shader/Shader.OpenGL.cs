using System;
using System.Collections.Generic;
using OpenTK.Graphics.ES20;
using Bool = OpenTK.Graphics.ES20.All;
namespace EW.Mobile.Platforms.Graphics
{

    
    internal partial class Shader
    {
        private string _glslCode;

        //
        private int _shaderHandler = -1;

        internal int GetShaderHandle()
        {
            if (_shaderHandler != -1)
                return _shaderHandler;
            _shaderHandler = GL.CreateShader(Stage == ShaderStage.Vertex ? ShaderType.VertexShader : ShaderType.FragmentShader);
            GraphicsExtensions.CheckGLError();
            GL.ShaderSource(_shaderHandler, _glslCode);
            GraphicsExtensions.CheckGLError();
            GL.CompileShader(_shaderHandler);
            GraphicsExtensions.CheckGLError();
            int compiled = 0;
            GL.GetShader(_shaderHandler, ShaderParameter.CompileStatus, out compiled);
            GraphicsExtensions.CheckGLError();
            if(compiled != (int)Bool.True)
            {
                if (GL.IsShader(_shaderHandler))
                {
                    GL.DeleteShader(_shaderHandler);
                    GraphicsExtensions.CheckGLError();
                }
                _shaderHandler = -1;

                throw new InvalidOperationException("Shader Compilation Failed");
            }
            return _shaderHandler;
        }


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
        /// 
        /// </summary>
        /// <param name="program"></param>
        internal void GetVertexAttributeLocations(int program)
        {
            for(int i = 0; i < Attributes.Length; i++)
            {
                Attributes[i].location = GL.GetAttribLocation(program, Attributes[i].name);
                GraphicsExtensions.CheckGLError();
            }
        }

        internal void ApplySamplerTextureUnits(int program)
        {
            foreach(var sampler in Samplers)
            {
                var loc = GL.GetUniformLocation(program, sampler.name);
                GraphicsExtensions.CheckGLError();
                if (loc != -1)
                {
                    GL.Uniform1(loc, sampler.textureSlot);
                    GraphicsExtensions.CheckGLError();
                }
            }
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
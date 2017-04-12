using System;
using System.Collections.Generic;
using OpenTK.Graphics.ES20;
using GetProgramParameterName = OpenTK.Graphics.ES20.ProgramParameter;
using Bool = OpenTK.Graphics.ES20.All;
namespace EW.Xna.Platforms.Graphics
{

    /// <summary>
    /// shader 程序对象
    /// 由多个着色器合并之后并最终链接完成
    /// </summary>
    internal class ShaderProgram
    {
        public readonly int Program;
        private readonly Dictionary<string, int> _uniformLocations = new Dictionary<string, int>();


        public ShaderProgram(int program)
        {
            Program = program;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetUnitformLocation(string name)
        {
            if (_uniformLocations.ContainsKey(name))
                return _uniformLocations[name];

            var location = GL.GetUniformLocation(Program, name);
            GraphicsExtensions.CheckGLError();
            _uniformLocations[name] = location;
            return location;
        }
    }


    /// <summary>
    /// shader 程序缓存
    /// </summary>
    internal class ShaderProgramCache:IDisposable
    {
        private readonly Dictionary<int, ShaderProgram> _programCache = new Dictionary<int, ShaderProgram>();
        bool disposed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertexShader"></param>
        /// <param name="pixelShader"></param>
        /// <returns></returns>
        public ShaderProgram GetProgram(Shader vertexShader,Shader pixelShader)
        {
            var key = vertexShader.HashKey | pixelShader.HashKey;
            if (!_programCache.ContainsKey(key))
            {
                _programCache.Add(key, Link(pixelShader, vertexShader));
            }
            return _programCache[key];
        }


        /// <summary>
        /// 链接着色器->着色器程序
        /// </summary>
        /// <param name="pixelShader"></param>
        /// <param name="vertexShader"></param>
        /// <returns></returns>
        private ShaderProgram Link(Shader pixelShader,Shader vertexShader)
        {
            var program = GL.CreateProgram();
            GraphicsExtensions.CheckGLError();

            GL.AttachShader(program, vertexShader.GetShaderHandle());
            GraphicsExtensions.CheckGLError();

            GL.AttachShader(program, pixelShader.GetShaderHandle());
            GraphicsExtensions.CheckGLError();

            GL.LinkProgram(program);
            GraphicsExtensions.CheckGLError();

            GL.UseProgram(program);
            GraphicsExtensions.CheckGLError();

            vertexShader.GetVertexAttributeLocations(program);

            pixelShader.ApplySamplerTextureUnits(program);

            var linked = 0;

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out linked);

            if(linked == (int)Bool.False)
            {
                GL.DetachShader(program, vertexShader.GetShaderHandle());
                GL.DetachShader(program, pixelShader.GetShaderHandle());

                GL.DeleteProgram(program);

                throw new InvalidOperationException("Unable to link effect program");
            }


            return new ShaderProgram(program);
        }
        /// <summary>
        /// 清理程序缓存，释放所有shader
        /// </summary>
        public void Clear()
        {
            foreach(var pair in _programCache)
            {
                if (GL.IsProgram(pair.Value.Program))
                {
                    GL.DeleteProgram(pair.Value.Program);
                    GraphicsExtensions.CheckGLError();
                }
            }

            _programCache.Clear();
        }


        ~ShaderProgramCache()
        {
            Dispose(false);   
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    Clear();
                disposed = true;
            }
        }

    }
}
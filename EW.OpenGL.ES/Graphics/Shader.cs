using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;
namespace EW.OpenGLES.Graphics
{
    class Shader:IShader
    {

        public const int VertexPosAttributeIndex = 0;
        public const int TexCoordAttributeIndex = 1;
        public const int TexMetadataAttributeIndex = 2;

        readonly Dictionary<string, int> samplers = new Dictionary<string, int>();
        readonly Dictionary<int, ITexture> textures = new Dictionary<int, ITexture>();
        readonly int program;

        public Shader(string name) {

            var vertexShader = CompileShaderObject(ShaderType.VertexShader, name);
            var fragmentShader = CompileShaderObject(ShaderType.FragmentShader, name);

            program = GL.CreateProgram();
            GraphicsExtensions.CheckGLError();
            GL.BindAttribLocation(program, VertexPosAttributeIndex, "aVertexPosition");
            GraphicsExtensions.CheckGLError();
            GL.BindAttribLocation(program, TexCoordAttributeIndex, "aVertexTexCoord");
            GraphicsExtensions.CheckGLError();
            GL.BindAttribLocation(program, TexMetadataAttributeIndex, "aVertexTexMetadata");
            GraphicsExtensions.CheckGLError();
            GL.AttachShader(program, vertexShader);
            GraphicsExtensions.CheckGLError();
            GL.AttachShader(program, fragmentShader);
            GraphicsExtensions.CheckGLError();
            GL.LinkProgram(program);
            GraphicsExtensions.CheckGLError();
            int success;
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out success);
            GraphicsExtensions.CheckGLError();
            if (success == 0)
            {
                var log = GL.GetProgramInfoLog(program);
                GraphicsExtensions.CheckGLError();
                GL.DetachShader(program, vertexShader);
                GraphicsExtensions.CheckGLError();
                GL.DetachShader(program, fragmentShader);
                GraphicsExtensions.CheckGLError();

                throw new InvalidOperationException("Unable to link effect program.");
            }

            GL.UseProgram(program);
            GraphicsExtensions.CheckGLError();
            int numUniforms;
            //GL.GetProgram(program, GetProgramParameterName.LogLength, out numUniforms);
            GL.GetProgram(program, GetProgramParameterName.ActiveUniforms, out numUniforms);
            GraphicsExtensions.CheckGLError();

            var nextTexUnit = 0;
            for (var i = 0; i < numUniforms; i++)
            {

                int length, size;
                int type;
                var sb = new StringBuilder(128);
                GL.GetActiveUniform(program, i, 128, out length, out size,out type,sb);
                GraphicsExtensions.CheckGLError();
                var sampler = sb.ToString();

                if (type == 35678) {
                    samplers.Add(sampler, nextTexUnit);

                    var loc = GL.GetUniformLocation(program, sampler);
                    GraphicsExtensions.CheckGLError();
                    GL.Uniform1(loc, nextTexUnit);
                    GraphicsExtensions.CheckGLError();
                    nextTexUnit++;
                }
            }

        }

        private int CompileShaderObject(ShaderType type, string name) {

            //var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var ext = type == ShaderType.VertexShader ? "vert" : "frag";
            var filename = Path.Combine("Content","glsl", name + "." + ext);
            //var filePath = Path.Combine(documentsPath, filename);
            var code = string.Empty;
            using (var reader = new StreamReader(Android.App.Application.Context.Assets.Open(filename)))
            {
                code = reader.ReadToEnd();
            }

            //System.Text.Encoding.ASCII.GetString(Android.App.Application.Context.Assets.Open(filename));

            var shader = GL.CreateShader(type);
            GraphicsExtensions.CheckGLError();
            
            //string vertexShader = "uniform mat4 u_MVPMatrix; \n" +
            //    "attribute vec4 a_Position;\n" +
            //    "attribute vec4 a_Color;\n" +
            //    "varying vec4 v_Color; \n" +
            //    "void main()    \n" +
            //    "{  \n" +
            //    "v_Color = a_Color; \n" +
            //    "gl_Position = u_MVPMatrix \n" +
            //    "*a_Position;   \n" +
            //    "}  \n";
            //code = vertexShader;
            unsafe
            {
                var length = code.Length;
                GL.ShaderSource2(shader,1,new string[] { code },new IntPtr(&length));
            }
            GraphicsExtensions.CheckGLError();
            GL.CompileShader(shader);
            GraphicsExtensions.CheckGLError();
            int compiled;
            GL.GetShader(shader, ShaderParameter.CompileStatus, out compiled);
            GraphicsExtensions.CheckGLError();
            if (compiled == 0)
            {
                var log = GL.GetShaderInfoLog(shader);
                GraphicsExtensions.CheckGLError();
                shader = -1;
                throw new InvalidOperationException("Shader Compilation Failed");
            }
            return shader;
        }

        public void Render(Action a)
        {
            Threading.EnsureUIThread();
            GL.UseProgram(program);

            //bind the textures
            foreach(var kv in textures)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + kv.Key);
                GL.BindTexture(TextureTarget.Texture2D, ((Texture)kv.Value).ID);
            }

            GraphicsExtensions.CheckGLError();
            a();
            GraphicsExtensions.CheckGLError();

        }


        public void SetVec(string name, float x)
        {
            Threading.EnsureUIThread();
            GL.UseProgram(program);
            GraphicsExtensions.CheckGLError();
            var param = GL.GetUniformLocation(program, name);
            GraphicsExtensions.CheckGLError();
            GL.Uniform1f(param, x);
            GraphicsExtensions.CheckGLError();
        }

        public void SetVec(string name,float x,float y)
        {

            Threading.EnsureUIThread();
            GL.UseProgram(program);
            GraphicsExtensions.CheckGLError();
            var param = GL.GetUniformLocation(program, name);
            GraphicsExtensions.CheckGLError();
            GL.Uniform2f(param, x, y);
            GraphicsExtensions.CheckGLError();
        }

        public void SetVec(string name,float x,float y,float z)
        {
            Threading.EnsureUIThread();
            GL.UseProgram(program);
            GraphicsExtensions.CheckGLError();
            var param = GL.GetUniformLocation(program, name);
            GraphicsExtensions.CheckGLError();
            GL.Uniform3f(param, x, y, z);
            GraphicsExtensions.CheckGLError();
        }

        public void SetVec(string name,float[] vec,int length)
        {
            Threading.EnsureUIThread();
            var param = GL.GetUniformLocation(program, name);
            GraphicsExtensions.CheckGLError();
            unsafe
            {
                fixed(float* pVec = vec)
                {
                    var ptr = new IntPtr(pVec);
                    switch (length)
                    {
                        case 1:GL.Uniform1fv(param, 1, ptr);break;
                        case 2:GL.Uniform2fv(param, 1, ptr);break;
                        case 3:GL.Uniform3fv(param, 1, ptr);break;
                        case 4:GL.Uniform4fv(param, 1, ptr);break;
                        default:throw new InvalidDataException("Invalid vector length");
                    }
                }
            }

            GraphicsExtensions.CheckGLError();
        }


        public void SetMatrix(string name, float[] mtx)
        {
            Threading.EnsureUIThread();
            if (mtx.Length != 16)
                throw new InvalidDataException("Invalid 4x4 matrix");

            GL.UseProgram(program);
            GraphicsExtensions.CheckGLError();
            var param = GL.GetUniformLocation(program, name);
            GraphicsExtensions.CheckGLError();

            unsafe
            {
                fixed (float* pMtx = mtx)
                    GL.UniformMatrix4fv(param, 1, false, new IntPtr(pMtx));
            }

            GraphicsExtensions.CheckGLError();
        }

        public void SetTexture(string name, ITexture t)
        {
            Threading.EnsureUIThread();
            if (t == null)
                return;

            int texUnit;
            if (samplers.TryGetValue(name, out texUnit))
                textures[texUnit] = t;
        }


        public void SetBool(string name, bool value)
        {
            Threading.EnsureUIThread();
            GL.UseProgram(program);
            GraphicsExtensions.CheckGLError();
            var param = GL.GetUniformLocation(program, name);
            GraphicsExtensions.CheckGLError();
            GL.Uniform1i(param, value ? 1 : 0);
            GraphicsExtensions.CheckGLError();
        }



    }
}
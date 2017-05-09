using System;
using OpenTK.Graphics.ES20;
using EW.Xna.Platforms.Utilities;
namespace EW.Xna.Platforms.Graphics
{
    internal partial class ConstantBuffer
    {

        private ShaderProgram _shaderProgram = null;

        private int _location;

        static ConstantBuffer _lastConstantBufferApplied = null;

        /// <summary>
        /// A hash value which can be used to compare constant buffers.
        /// </summary>

        internal int HashKey { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        private void PlatformInitialize()
        {
            var data = new byte[_parameters.Length];
            for(var i = 0; i < _parameters.Length; i++)
            {
                unchecked
                {
                    data[i] = (byte)(_parameters[i] | _offsets[i]);
                }
            }
            HashKey = Hash.ComputeHash(data);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="program"></param>
        public unsafe void PlatformApply(GraphicsDevice device,ShaderProgram program)
        {
            //If the program changed then lookup the uniform again and apply the state
            if(_shaderProgram != program)
            {
                var location = program.GetUnitformLocation(_name);
                if (location == -1)
                    return;
                _shaderProgram = program;
                _location = location;
                _dirty = true;
            }
            // If the shader program is the same,the effect may still be different and have different values in the buffer
            if (!Object.ReferenceEquals(this, _lastConstantBufferApplied))
                _dirty = true;

            //If the buffer content hasn't changed then we're done...use the previously set uniform state.
            if (!_dirty)
                return;

            fixed(byte* bytePtr = _buffer)
            {
                GL.Uniform4(_location, _buffer.Length / 16, (float*)bytePtr);
                GraphicsExtensions.CheckGLError();
            }

            _dirty = false;
            _lastConstantBufferApplied = this;
        }


        private void PlatformClear()
        {
            _shaderProgram = null;
        }

        

    }
}
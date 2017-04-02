using System;
using OpenTK.Graphics.ES20;
namespace EW.Mobile.Platforms.Graphics
{
    internal partial class ConstantBuffer
    {

        private ShaderProgram _shaderProgram = null;

        private int _location;

        static ConstantBuffer _lastConstantBufferApplied = null;

        /// <summary>
        /// 
        /// </summary>

        internal int HashKey { get; private set; }

        private void PlatformInitialize()
        {

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="program"></param>
        public unsafe void PlatformApply(GraphicsDevice device,ShaderProgram program)
        {
            if(_shaderProgram != program)
            {
                var location = program.GetUnitformLocation(_name);
                if (location == -1)
                    return;
                _shaderProgram = program;
                _location = location;
                _dirty = true;
            }

            if (!Object.ReferenceEquals(this, _lastConstantBufferApplied))
                _dirty = true;

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
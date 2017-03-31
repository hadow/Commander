using System;

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
        }

    }
}
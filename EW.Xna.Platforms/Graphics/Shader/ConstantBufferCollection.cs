using System;


namespace EW.Xna.Platforms.Graphics
{
    /// <summary>
    /// Added ConstantBufferCollection  to better manage applying constant buffers.
    /// </summary>
    internal sealed class ConstantBufferCollection
    {

        private readonly ConstantBuffer[] _buffers;

        private ShaderStage _stage;

        private int _valid;

        internal ConstantBufferCollection(ShaderStage stage,int maxBuffers)
        {
            _stage = stage;
            _buffers = new ConstantBuffer[maxBuffers];
            _valid = 0;
        }

        public ConstantBuffer this[int index]
        {
            get { return _buffers[index]; }
            set
            {
                if (_buffers[index] == value)
                    return;
                if (value != null)
                {
                    _buffers[index] = value;
                    _valid |= 1 << index;
                }
                else
                {
                    _buffers[index] = null;
                    _valid &= ~(1 << index);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="shaderProgram"></param>

        internal void SetConstantBuffers(GraphicsDevice device,ShaderProgram shaderProgram)
        {
            if (_valid == 0)
                return;

            var valid = _valid;

            for(int i = 0; i < _buffers.Length; i++)
            {
                var buffer = _buffers[i];
                if(buffer!=null && !buffer.IsDisposed)
                {
#if OPENGL
                    buffer.PlatformApply(device, shaderProgram);
#endif
                }

                //如果这是最后一个
                valid &= ~(1 << i);
                if (valid == 0)
                    return;
            }
        }

        internal void Clear()
        {
            for (var i = 0; i < _buffers.Length; i++)
                _buffers[i] = null;

            _valid = 0;
        }


    }
}
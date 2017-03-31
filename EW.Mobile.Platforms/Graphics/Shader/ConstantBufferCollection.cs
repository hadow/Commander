using System;


namespace EW.Mobile.Platforms.Graphics
{
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

                }
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
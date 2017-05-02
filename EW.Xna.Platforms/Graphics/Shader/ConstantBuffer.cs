using System;


namespace EW.Xna.Platforms.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    internal partial class ConstantBuffer:GraphicsResource
    {

        private readonly byte[] _buffer;

        private readonly int[] _parameters;

        private readonly int[] _offsets;

        private readonly string _name;

        private ulong _stateKey;

        private bool _dirty;

        public ConstantBuffer(GraphicsDevice device,int sizeInBytes,int[] parameterIndexes,int[] parameterOffsets,string name)
        {
            GraphicsDevice = device;
            _buffer = new byte[sizeInBytes];
            _parameters = parameterIndexes;
            _offsets = parameterOffsets;
            _name = name;
            PlatformInitialize();
        }

        public ConstantBuffer(ConstantBuffer cloneSource)
        {
            GraphicsDevice = cloneSource.GraphicsDevice;

            _name = cloneSource._name;
            _parameters = cloneSource._parameters;
            _offsets = cloneSource._offsets;

            //
            _buffer = (byte[])cloneSource._buffer.Clone();
            PlatformInitialize();
        }


        internal void Clear()
        {
            PlatformClear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        public void Update(EffectParameterCollection parameters)
        {
            if (_stateKey > EffectParameter.NextStateKey)
                _stateKey = 0;

            for(var p = 0; p < _parameters.Length; p++)
            {
                var index = _parameters[p];
                var param = parameters[index];
                if (param.StateKey < _stateKey)
                    continue;

                var offset = _offsets[p];
                _dirty = true;

                SetParameter(offset, param);
                
            }

            _stateKey = EffectParameter.NextStateKey;
        }

        private int SetParameter(int offset,EffectParameter param)
        {
            var rowsUsed = 0;
            return rowsUsed;
        }
    }
}
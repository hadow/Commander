using System;


namespace EW.Xna.Platforms.Graphics
{
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


        internal void Clear()
        {
            PlatformClear();
        }
    }
}
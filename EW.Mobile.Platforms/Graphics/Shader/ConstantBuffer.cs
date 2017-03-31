using System;


namespace EW.Mobile.Platforms.Graphics
{
    internal partial class ConstantBuffer:GraphicsResource
    {

        private readonly byte[] _buffer;

        private readonly byte[] _parameters;

        private readonly int[] _offsets;

        private readonly string _name;

        private ulong _stateKey;

        private bool _dirty;


    }
}
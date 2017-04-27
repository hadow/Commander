using System;


namespace EW.Xna.Platforms.Graphics
{
    public sealed partial class SamplerStateCollection
    {
        private readonly GraphicsDevice _graphicsDevice;

        private readonly SamplerState[] _samplers;
        private readonly SamplerState[] _actualSamplers;

        private readonly bool _applyToVertexStage;

        internal SamplerStateCollection(GraphicsDevice device,int maxSamplers,bool applyToVertexStage)
        {
            _graphicsDevice = device;

            _samplers = new SamplerState[maxSamplers];
            _actualSamplers = new SamplerState[maxSamplers];
            _applyToVertexStage = applyToVertexStage;

        }

        public SamplerState this[int index]
        {
            get { return _samplers[index]; }
            set
            {
                if (value == null)
                    throw new ArgumentException("value");

                if (_samplers[index] == value)
                    return;

                _samplers[index] = value;
            }
        }
    }
}
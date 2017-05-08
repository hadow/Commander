using System;


namespace EW.Xna.Platforms.Graphics
{

    /// <summary>
    /// 
    /// </summary>
    public sealed partial class SamplerStateCollection
    {
        private readonly GraphicsDevice _graphicsDevice;

        private readonly SamplerState _samplerStateAnisotropicClamp;
        private readonly SamplerState _samplerStateAnisotropicWrap;
        private readonly SamplerState _samplerStateLinearClamp;
        private readonly SamplerState _samplerStateLinearWrap;
        private readonly SamplerState _samplerStatePointClamp;
        private readonly SamplerState _samplerStatePointWrap;


        private readonly SamplerState[] _samplers;
        private readonly SamplerState[] _actualSamplers;

        private readonly bool _applyToVertexStage;

        internal SamplerStateCollection(GraphicsDevice device,int maxSamplers,bool applyToVertexStage)
        {
            _graphicsDevice = device;


            _samplerStateAnisotropicClamp = SamplerState.AnisotropicClamp.Clone();
            _samplerStateAnisotropicWrap = SamplerState.AnisotropicWrap.Clone();
            _samplerStateLinearClamp = SamplerState.LinearClamp.Clone();
            _samplerStateLinearWrap = SamplerState.LinearWrap.Clone();
            _samplerStatePointClamp = SamplerState.PointClamp.Clone();
            _samplerStatePointWrap = SamplerState.PointWrap.Clone();


            _samplers = new SamplerState[maxSamplers];
            _actualSamplers = new SamplerState[maxSamplers];
            _applyToVertexStage = applyToVertexStage;

            Clear();
                
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

                var newSamplerState = value;

                if (ReferenceEquals(value, SamplerState.AnisotropicClamp))
                    newSamplerState = _samplerStateAnisotropicClamp;
                else if (ReferenceEquals(value, SamplerState.AnisotropicWrap))
                    newSamplerState = _samplerStateAnisotropicWrap;
                else if (ReferenceEquals(value, SamplerState.LinearClamp))
                    newSamplerState = _samplerStateLinearClamp;
                else if (ReferenceEquals(value, SamplerState.LinearWrap))
                    newSamplerState = _samplerStateLinearWrap;
                else if (ReferenceEquals(value, SamplerState.PointClamp))
                    newSamplerState = _samplerStatePointClamp;
                else if (ReferenceEquals(value, SamplerState.PointWrap))
                    newSamplerState = _samplerStatePointWrap;

                newSamplerState.BindToGraphicsDevice(_graphicsDevice);
                _actualSamplers[index] = newSamplerState;
                PlatformSetSamplerState(index);
            }
        }

        internal void Clear()
        {
            for(var i = 0; i < _samplers.Length; i++)
            {
                _samplers[i] = SamplerState.LinearWrap;

                _samplerStateLinearWrap.BindToGraphicsDevice(_graphicsDevice);
                _actualSamplers[i] = _samplerStateLinearWrap;
                
            }

            PlatformClear();
        }
    }
}
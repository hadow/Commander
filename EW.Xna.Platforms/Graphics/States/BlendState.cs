using System;


namespace EW.Xna.Platforms.Graphics
{

    public enum Blend
    {
        /// <summary>
        /// Each component of the color is multiplied by {1,1,1,1}.
        /// </summary>
        One,
        Zero,
        SourceColor,
        /// <summary>
        /// Each component of the color is multiplied by the inverse of the alpha value of the source.
        /// {1-AS,1-AS,1-AS,1-AS},where As is  the source alpha value.
        /// </summary>
        InverseSourceColor,
        SourceAlpha,
        InverseSourceAlpha,
        DestinationColor,
        InverseDestinationColor,
        DestinationAlpha,
        InverseDestinationAlpha,
        BlendFactor,
        InverseBlendFactor,
        SourceAlphaSaturation,//±¥ºÍ¶È
    }


    public enum BlendFunction
    {
        Add,//(srcColor * srcBlend) + (destColor * destBlend)
        Subtract,//(srcColor * srcBlend) - (destColor * destBlend)
        ReverseSubtrace,//(destColor * destBlend) - (srcColor * srcBlend);
        Min,    //Min((destColor * destBlend),(srcColor * srcBlend));
        Max     //Max((destColor * destBlend),(srcColor * srcBlend));
    }


    /// <summary>
    /// 
    /// </summary>
    public partial class BlendState:GraphicsResource
    {

        public static readonly BlendState Additive;
        public static readonly BlendState AlphaBlend;
        public static readonly BlendState NonPremultiplied;
        public static readonly BlendState Opaque;

        private readonly bool _defaultStateObject;

        private readonly TargetBlendState[] _targetBlendState;

        private Color _blendFactor;

        private int _multiSampleMask;
        private bool _independentBlendEnable;
        public Color BlendFactor
        {
            get { return _blendFactor; }
            set
            {
                ThrowIfBound();
                _blendFactor = value;
            }
        }

        public BlendState()
        {
            _targetBlendState = new TargetBlendState[4];
            _targetBlendState[0] = new TargetBlendState(this);
            _targetBlendState[1] = new TargetBlendState(this);
            _targetBlendState[2] = new TargetBlendState(this);
            _targetBlendState[3] = new TargetBlendState(this);

            _blendFactor = Color.White;
            _multiSampleMask = Int32.MaxValue;
            _independentBlendEnable = false;
            
        }

        public BlendState(string name,Blend sourceBlend,Blend destinationBlend):this()
        {
            Name = name;

            ColorSourceBlend = sourceBlend;
            AlphaSourceBlend = sourceBlend;

            ColorDestinationBlend = destinationBlend;
            AlphaDestinationBlend = destinationBlend;

            _defaultStateObject = true;
        }

        private BlendState(BlendState cloneSource)
        {
            Name = cloneSource.Name;

            _targetBlendState = new TargetBlendState[4];
            _targetBlendState[0] = cloneSource[0].Clone(this);
            _targetBlendState[1] = cloneSource[1].Clone(this);
            _targetBlendState[2] = cloneSource[2].Clone(this);
            _targetBlendState[3] = cloneSource[3].Clone(this);

            _blendFactor = cloneSource._blendFactor;
            _multiSampleMask = cloneSource._multiSampleMask;
            _independentBlendEnable = cloneSource._independentBlendEnable;
            
        }

        static BlendState()
        {
            Additive = new BlendState("BlendState.Additive", Blend.SourceAlpha, Blend.One);
            AlphaBlend = new BlendState("BlendState.AlphaBlend", Blend.One, Blend.InverseSourceAlpha);
            NonPremultiplied = new BlendState("BlendState.NonPremultiplied", Blend.SourceAlpha, Blend.InverseSourceAlpha);
            Opaque = new BlendState("BlendState.Opaue", Blend.One, Blend.Zero);
        }

        public TargetBlendState this[int index]
        {
            get { return _targetBlendState[index];  }
        }

        internal BlendState Clone()
        {
            return new BlendState(this);
        }

        internal void BindToGraphicsDevice(GraphicsDevice device)
        {
            if (_defaultStateObject)
                throw new InvalidOperationException("You Cannot bind a default state object");
            if (GraphicsDevice != null && GraphicsDevice != device)
                throw new InvalidOperationException("This blend state is already bound to a different graphics device");
            GraphicsDevice = device;
        }
        

        public BlendFunction AlphaBlendFunction
        {
            get { return _targetBlendState[0].AlphaBlendFunction; }
            set
            {
                ThrowIfBound();
                _targetBlendState[0].AlphaBlendFunction = value;
            }
        }

        public Blend AlphaDestinationBlend
        {
            get { return _targetBlendState[0].AlphaDestinationBlend; }
            set
            {
                ThrowIfBound();
                _targetBlendState[0].AlphaDestinationBlend = value;
            }
        }

        public Blend AlphaSourceBlend
        {
            get { return _targetBlendState[0].AlphaSourceBlend; }
            set
            {
                ThrowIfBound();
                _targetBlendState[0].AlphaSourceBlend = value;
            }
        }

        public BlendFunction ColorBlendFunction
        {
            get { return _targetBlendState[0].ColorBlendFunction; }
            set
            {
                ThrowIfBound();
                _targetBlendState[0].ColorBlendFunction = value;
            }
        }

        public Blend ColorSourceBlend
        {
            get { return _targetBlendState[0].ColorSourceBlend; }
            set
            {
                ThrowIfBound();
                _targetBlendState[0].ColorSourceBlend = value;
            }
        }

        public Blend ColorDestinationBlend
        {
            get { return _targetBlendState[0].ColorDestinationBlend; }
            set
            {
                ThrowIfBound();
                _targetBlendState[0].ColorDestinationBlend = value;
            }
        }

        public ColorWriteChannels ColorWriteChannels
        {
            get { return _targetBlendState[0].ColorWriteChannels; }
            set
            {
                ThrowIfBound();
                _targetBlendState[0].ColorWriteChannels = value;
            }
        }
        public ColorWriteChannels ColorWriteChannels1
        {
            get { return _targetBlendState[1].ColorWriteChannels; }
            set
            {
                ThrowIfBound();
                _targetBlendState[1].ColorWriteChannels = value;
            }
        }
        public ColorWriteChannels ColorWriteChannels2
        {
            get { return _targetBlendState[2].ColorWriteChannels; }
            set
            {
                ThrowIfBound();
                _targetBlendState[2].ColorWriteChannels = value;
            }
        }

        public ColorWriteChannels ColorWriteChannels3
        {
            get { return _targetBlendState[3].ColorWriteChannels; }
            set
            {
                ThrowIfBound();
                _targetBlendState[3].ColorWriteChannels = value;
            }
        }


        internal void ThrowIfBound()
        {
            if (_defaultStateObject)
                throw new InvalidOperationException("You cannot modify a default blend state");

            if (GraphicsDevice != null)
                throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");
        }
    }
}
using System;


namespace EW.Xna.Platforms.Graphics
{

    public enum TextureFilter
    {
        Linear,
        Point,
        Anistropic,//在大多数情况下，实现更好地锐化纹理的方法是可以利用各向异性过滤。
        LinearMipPoint,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum TextureAddressMode
    {
        Wrap,
        Clamp,
        Mirror,
        Border,
    }


    /// <summary>
    /// 纹理采样状态
    /// </summary>
    public partial class SamplerState:GraphicsResource
    {
        private readonly bool _defaultStateObject;

        public static readonly SamplerState AnisotropicClamp;
        public static readonly SamplerState AnisotropicWrap;
        public static readonly SamplerState LinearClamp;
        public static readonly SamplerState LinearWrap;
        public static readonly SamplerState PointClamp;
        public static readonly SamplerState PointWrap;

        static SamplerState()
        {
            AnisotropicClamp = new SamplerState("SamplerState.AnisotropicClamp", TextureFilter.Anistropic,TextureAddressMode.Clamp);
            AnisotropicWrap = new SamplerState("SamplerState.AnisotropicWrap", TextureFilter.Anistropic, TextureAddressMode.Wrap);
            LinearClamp = new SamplerState("SamplerState.LinearClamp", TextureFilter.Linear, TextureAddressMode.Clamp);
            LinearWrap = new SamplerState("SamplerState.LinearWrap", TextureFilter.Linear, TextureAddressMode.Wrap);
            PointClamp = new SamplerState("SamplerState.PointClamp", TextureFilter.Point, TextureAddressMode.Clamp);
            PointWrap = new SamplerState("SamplerState.PointWrap", TextureFilter.Point, TextureAddressMode.Wrap);
        }

        public SamplerState()
        {
            Filter = TextureFilter.Linear;
            AddressU = TextureAddressMode.Wrap;
            AddressV = TextureAddressMode.Wrap;
            AddressW = TextureAddressMode.Wrap;

            
                 
        }

        private SamplerState(string name,TextureFilter filter,TextureAddressMode addressMode):this()
        {
            Name = name;
            _filter = filter;
            _addressU = addressMode;
            _addressV = addressMode;
            _addressW = addressMode;
            _defaultStateObject = true;
        }

        private TextureFilter _filter;

        public TextureFilter Filter { get { return _filter; }
        set
            {
                _filter = value;
            }
        }

        private TextureAddressMode _addressU;

        public TextureAddressMode AddressU
        {
            get { return _addressU; }
            set
            {
                _addressU = value;
            }
        }
        private TextureAddressMode _addressV;

        public TextureAddressMode AddressV
        {
            get { return _addressV; }
            set { _addressV = value; }
        }
        private TextureAddressMode _addressW;

        public TextureAddressMode AddressW
        {
            get { return _addressW; }
            set
            {
                _addressW = value;
            }
        }

        private Color _borderColor;
        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                ThrowIfBound();
                _borderColor = value;
            }
        }

        private int _maxAnisotropy;
        public int MaxAnisotropy
        {
            get { return _maxAnisotropy; }
            set
            {
                ThrowIfBound();
                _maxAnisotropy = value;
            }
        }


        private int _maxMipLevel;
        public int MaxMipLevel
        {
            get { return _maxMipLevel; }
            set
            {
                ThrowIfBound();
                _maxMipLevel = value;
            }
        }

        private float _mipMapLevelOfDetailBias;
        public float MipMapLevelOfDetailBias
        {
            get { return _mipMapLevelOfDetailBias; }
            set
            {
                ThrowIfBound();
                _mipMapLevelOfDetailBias = value;
            }
        }

        internal void ThrowIfBound()
        {
            if (_defaultStateObject)
                throw new InvalidOperationException("You cannot modify a default sampler state object.");

            if (GraphicsDevice != null)
            {
                throw new InvalidOperationException("You cannot modify the sampler state after it has been bound to the graphics device!");
            }
        }
    }
}
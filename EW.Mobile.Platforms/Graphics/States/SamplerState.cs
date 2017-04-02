using System;


namespace EW.Mobile.Platforms.Graphics
{

    public enum TextureFilter
    {
        Linear,
        Point,
    }

    public enum TextureAddressMode
    {
        Wrap,
        Clamp,
        Mirror,
        Border,
    }


    /// <summary>
    /// ÎÆÀí²ÉÑù×´Ì¬
    /// </summary>
    public partial class SamplerState:GraphicsResource
    {
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

        public TextureAddressMode AdressW
        {
            get { return _addressW; }
            set
            {
                _addressW = value;
            }
        }
    }
}
using System;


namespace EW.Xna.Platforms.Graphics
{
    /// <summary>
    /// ÑÕÉ«Í¨µÀ
    /// </summary>
    public enum ColorWriteChannels
    {
        None = 0,
        Red = 1,
        Green = 2,
        Blue = 4,
        Alpha = 8,
        All = 15,
    }

    /// <summary>
    /// 
    /// </summary>
    public class TargetBlendState
    {

        private readonly BlendState _parent;
        private BlendFunction _alphaBlendFunction;
        private Blend _alphaDestinationBlend;
        private Blend _alphaSourceBlend;

        private BlendFunction _colorBlendFunction;
        private Blend _colorDestinationBlend;
        private Blend _colorSourceBlend;

        private ColorWriteChannels _colorWriteChannels;

        internal TargetBlendState(BlendState parent)
        {
            _parent = parent;
            AlphaBlendFunction = BlendFunction.Add;
            AlphaDestinationBlend = Blend.Zero;
            AlphaSourceBlend = Blend.One;

            ColorBlendFunction = BlendFunction.Add;
            ColorDestinationBlend = Blend.Zero;
            ColorSourceBlend = Blend.One;

            ColorWriteChannels = ColorWriteChannels.All;
        }

        public BlendFunction AlphaBlendFunction
        {
            get { return _alphaBlendFunction; }
            set
            {
                _parent.ThrowIfBound();
                _alphaBlendFunction = value;
            }
        }

        public Blend AlphaDestinationBlend
        {
            get { return _alphaDestinationBlend; }
            set
            {
                _parent.ThrowIfBound();
                _alphaDestinationBlend = value;
            }
        }

        public Blend AlphaSourceBlend
        {
            get { return _alphaSourceBlend; }
            set
            {
                _parent.ThrowIfBound();
                _alphaSourceBlend = value;
            }
        }



        public BlendFunction ColorBlendFunction
        {
            get { return _colorBlendFunction; }
            set
            {
                _parent.ThrowIfBound();
                _colorBlendFunction = value;
            }
        }

        public Blend ColorDestinationBlend
        {
            get { return _colorDestinationBlend; }
            set
            {
                _parent.ThrowIfBound();
                _colorDestinationBlend = value;
            }
        }

        public Blend ColorSourceBlend
        {
            get { return _colorSourceBlend; }
            set
            {
                _parent.ThrowIfBound();
                _colorSourceBlend = value;
            }
        }

        public ColorWriteChannels ColorWriteChannels
        {
            get { return _colorWriteChannels; }
            set
            {
                _parent.ThrowIfBound();
                _colorWriteChannels = value;
            }
        }

        internal TargetBlendState Clone(BlendState parent)
        {
            return new TargetBlendState(parent)
            {
                AlphaBlendFunction = AlphaBlendFunction,
                AlphaDestinationBlend = AlphaDestinationBlend,
                AlphaSourceBlend = AlphaSourceBlend,
                ColorBlendFunction = ColorBlendFunction,
                ColorDestinationBlend = ColorDestinationBlend,
                ColorSourceBlend = ColorSourceBlend,
                ColorWriteChannels = ColorWriteChannels,
            };
        }





    }
}
using System;


namespace EW.Xna.Platforms.Graphics
{

    public enum FillMode
    {
        Solid,
        WireFrame,
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class RasterizerState:GraphicsResource
    {

        private readonly bool _defaultStateObject;


        public static readonly RasterizerState CullClockwise;
        public static readonly RasterizerState CullCounterClockwise;
        public static readonly RasterizerState CullNone;
        public RasterizerState() { }
        private RasterizerState(RasterizerState cloneSource)
        {
            Name = cloneSource.Name;
        }

        private RasterizerState(string name,CullMode cullMode):this()
        {

        }

        static RasterizerState()
        {
            CullClockwise = new RasterizerState("RasterizerState.CullClockwise", CullMode.CullClockwiseFace);
            CullCounterClockwise = new RasterizerState("RasterizerState.CullCounterClockwise", CullMode.CullCounterClockwiseFace);
            CullNone = new RasterizerState("RasterizerState.CullNone", CullMode.None);
        }

        internal void ThrowIfBound()
        {
            if (_defaultStateObject)
                throw new InvalidOperationException("You cannot modify a default rasterizer state object");
            if (GraphicsDevice != null)
                throw new InvalidOperationException("You cannot modify the rasterizer state after it has been bound to the graphics device!");
        }


        private CullMode _cullMode;

        public CullMode CullMode
        {
            get { return _cullMode; }
            set
            {
                _cullMode = value;
            }
        }


        private bool _scissorTestEnable;

        public bool ScissorTestEnable
        {
            get { return _scissorTestEnable; }
            set
            {
                _scissorTestEnable = value;
            }
        }

        private float _slopeScaleDepthBias;

        private bool _depthClipEnable;

        public bool DepthClipEnable
        {
            get { return _depthClipEnable; }
            set
            {
                ThrowIfBound();
                _depthClipEnable = value;
            }
        }


        private FillMode _fillMode;
        public FillMode FillMode
        {
            get { return _fillMode; }
            set
            {
                ThrowIfBound();
                _fillMode = value;
            }
        }

        internal void BindToGraphicsDevice(GraphicsDevice device)
        {
            if (_defaultStateObject)
                throw new InvalidOperationException("You cannot bind a default state object.");

            if (GraphicsDevice != null && GraphicsDevice != device)
                throw new InvalidOperationException("This rasterizer state is already bound to a different graphics device.");

            GraphicsDevice = device;
        }

        internal RasterizerState Clone()
        {
            return new RasterizerState(this);
        }
    }
}
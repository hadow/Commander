using System;


namespace EW.Xna.Platforms.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RasterizerState:GraphicsResource
    {

        private readonly bool _defaultStateObject;


        public static readonly RasterizerState CullClockwise;
        public static readonly RasterizerState CullCounterClockwise;
        public static readonly RasterizerState CullNone;
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

        internal void BindToGraphicsDevice(GraphicsDevice device)
        {
            if (_defaultStateObject)
                throw new InvalidOperationException("You cannot bind a default state object.");

            if (GraphicsDevice != null && GraphicsDevice != device)
                throw new InvalidOperationException("This rasterizer state is already bound to a different graphics device.");

            GraphicsDevice = device;
        }
    }
}
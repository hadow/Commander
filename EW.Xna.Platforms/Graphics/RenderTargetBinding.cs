using System;

namespace EW.Xna.Platforms.Graphics
{
    public struct RenderTargetBinding
    {

        private readonly Texture _renderTarget;

        public Texture RenderTarget { get { return _renderTarget; } }

        private readonly int _arraySlice;

        public int ArraySlic { get { return _arraySlice; } }

        private DepthFormat _depthFormat;

        internal DepthFormat DepthFormat { get { return _depthFormat; } }


        public RenderTargetBinding(RenderTarget2D renderTarget)
        {
            if (renderTarget == null)
                throw new ArgumentException("renderTarget");

            _renderTarget = renderTarget;
            _arraySlice = 0;
            _depthFormat = renderTarget.DepthStencilFormat;
        }

        public static implicit operator RenderTargetBinding(RenderTarget2D renderTarget)
        {
            return new RenderTargetBinding(renderTarget);
        }

    }
}
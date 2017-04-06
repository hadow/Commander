using System;


namespace EW.Mobile.Platforms.Graphics
{
    public partial class RenderTarget2D:Texture2D,IRenderTarget
    {
        public DepthFormat DepthStencilFormat { get; private set; }

        public int MultiSampleCount { get; private set; }

        public RenderTargetUsage RenderTargetUsage { get; private set; }

        public RenderTarget2D(GraphicsDevice graphicsDevice,int width,int height,bool mipMap,
            SurfaceFormat preferredFormat,DepthFormat preferredDepthFormat,int preferredMultiSampleCount,
            RenderTargetUsage usage,bool shared,int arraySize) : base(graphicsDevice, width, height, mipMap, preferredFormat, SurfaceType.RenderTarget, shared, arraySize)
        {
            DepthStencilFormat = preferredDepthFormat;
            MultiSampleCount = preferredMultiSampleCount;
            RenderTargetUsage = usage;

            PlatformConstruct(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage, shared);

        }








    }
}
using System;


namespace EW.Xna.Platforms.Graphics
{
    public partial class RenderTarget2D:Texture2D,IRenderTarget
    {
        public DepthFormat DepthStencilFormat { get; private set; }

        public int MultiSampleCount { get; private set; }

        /// <summary>
        /// Gets the usage mode of the render target.
        /// </summary>
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

        public RenderTarget2D(GraphicsDevice graphicsDevice,int width,int height,bool mipMap,SurfaceFormat preferredFormat,DepthFormat preferredDepthFormat,int preferredMultiSampleCount,RenderTargetUsage usage,bool shared)
            : this(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage, shared, 1) { }

        public RenderTarget2D(GraphicsDevice graphicsDevice,int width,int height,bool mipMap,SurfaceFormat preferredFormat,DepthFormat preferredDepthFormat,int preferredMultiSampleCount,RenderTargetUsage usage)
            : this(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage, false) { }


        public RenderTarget2D(GraphicsDevice graphicsDevice,int width,int height) : this(graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents) { }





    }
}
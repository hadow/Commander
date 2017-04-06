using System;
using OpenTK.Graphics.ES20;
namespace EW.Mobile.Platforms.Graphics
{
    public partial class RenderTarget2D
    {
        

        private void PlatformConstruct(GraphicsDevice graphicsDevice,int width,int height,bool mipMap,
            SurfaceFormat preferredFormat,DepthFormat preferredDepthFormat,int preferredMultiSampleCount,RenderTargetUsage usage,bool shared)
        {
            Threading.BlockOnUIThread(() =>
            {
                graphicsDevice.PlatformCreateRenderTarget(this, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage);
            });
        }

        int IRenderTarget.GLTexture { get { return glTexture; } }

        TextureTarget IRenderTarget.GLTarget
        {
            get { return glTarget; }
        }

        int IRenderTarget.GLColorBuffer { get; set; }

        int IRenderTarget.GLDepthBuffer { get; set; }

        int IRenderTarget.GLStencilBuffer { get; set; }

        TextureTarget IRenderTarget.GetFramebufferTarget(RenderTargetBinding renderTargetBinding)
        {
            return glTarget;
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                Threading.BlockOnUIThread(() =>
                {
                });
            }
            base.Dispose(disposing);
        }
    }
}
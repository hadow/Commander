using System;
using OpenTK.Graphics.ES20;

namespace EW.Xna.Platforms.Graphics
{

    public enum RenderTargetUsage
    {
        /// <summary>
        /// The render traget content will not be preserved.
        /// 渲染目标内容将不会保存
        /// </summary>
        DiscardContents,

        /// <summary>
        /// The render target content will be preserved even if it is slow or requires extra memory.
        /// 即使缓慢或需要分配额外的内存，渲染目标内容将被保留
        /// </summary>
        PreserveContents,

        /// <summary>
        /// The render target content might be preserved if the platform can do so without a penalty in performance or memory usage.
        /// </summary>
        PlatformContents,

    }


    /// <summary>
    /// 渲染目标
    /// </summary>
    internal interface IRenderTarget
    {

        /// <summary>
        /// 
        /// </summary>
        int Width { get; }

        int Height { get; }

        RenderTargetUsage RenderTargetUsage { get; }


        int GLTexture { get; }

        TextureTarget GLTarget { get; }

        int GLColorBuffer { get; set; }

        int GLDepthBuffer { get; set; }

        int GLStencilBuffer { get; set; }

        int MultiSampleCount { get; }

        int LevelCount { get; }

        TextureTarget GetFramebufferTarget(RenderTargetBinding renderTargetBinding);

    }
}
using System;
using OpenTK.Graphics.ES20;

namespace EW.Mobile.Platforms.Graphics
{

    public enum RenderTargetUsage
    {
        /// <summary>
        /// 渲染目标内容将不会保存
        /// </summary>
        DiscardContents,

        /// <summary>
        /// 
        /// </summary>
        PreserveContents,

        /// <summary>
        /// 
        /// </summary>
        PlatformContents,

    }
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


    }
}
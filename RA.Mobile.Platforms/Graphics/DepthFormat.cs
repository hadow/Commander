using System;
using System.Collections.Generic;


namespace RA.Mobile.Platforms.Graphics
{
    public enum DepthFormat
    {
        None,
        Depth16,
        Depth24,
        /// <summary>
        /// 32-bit depth-stencil buffer,where 24-bit depth and 8-bit for stencil used;
        /// </summary>
        Depth24Stencil8,
    }
}
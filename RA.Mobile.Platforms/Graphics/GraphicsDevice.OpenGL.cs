using System;
using System.Collections.Generic;

namespace RA.Mobile.Platforms.Graphics
{
    public partial class GraphicsDevice
    {
        /// <summary>
        /// 平台初始化
        /// </summary>
        private void PlatformInitialize()
        {
            _viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);

        }


    }
}
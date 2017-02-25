using System;
using System.Collections.Generic;

namespace RA.Mobile.Platforms.Graphics
{
    /// <summary>
    /// 描述参数
    /// </summary>
    public class PresentationParameters:IDisposable
    {
        public const int DefaultPresentRate = 60;//默认帧率

        private bool _isFullScreen;
        public bool IsFullScreen
        {
            get
            {
                return _isFullScreen;
            }
            set
            {
                _isFullScreen = value;
            }
        }
        private int _backBufferWidth = GraphicsDeviceManager.DefaultBackBufferWidth;
        private int _backBufferHeight = GraphicsDeviceManager.DefaultBackBufferHeight;

        public int BackBufferWidth
        {
            get { return _backBufferWidth; }
            set { _backBufferWidth = value; }
        }

        public int BackBufferHeight
        {
            get { return _backBufferHeight; }
            set { _backBufferHeight = value; }
        }
        private IntPtr deviceWindowHandle;


        public void Dispose() { }

    }
}
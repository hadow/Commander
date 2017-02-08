using System;
using RA.Mobile.Platforms.Graphics;

#if ANDROID
using Android.Views;
#endif
namespace RA.Mobile.Platforms
{
    /// <summary>
    /// 
    /// </summary>
    public class GraphicsDeviceManager:IGraphicsDeviceManager,IGraphicsDeviceService,IDisposable
    {
        readonly Game _game;
        private GraphicsDevice _graphicsDevice;
        private DisplayOrientation _supportedOrientation;

        private int _preferredBackBufferWidth;
        private int _preferredBackBufferHeight;

        public static readonly int DefaultBackBufferWidth = 800;
        public static readonly int DefaultBackBufferHeight = 480;

        private bool _drawBegun;
        bool disposed;
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return _graphicsDevice;
            }
        }
        public GraphicsDeviceManager(Game game)
        {
            if (game == null)
                throw new ArgumentNullException("The game cannot be null");
            _game = game;
            _supportedOrientation = DisplayOrientation.Default;

            if (_game.Services.GetService(typeof(IGraphicsDeviceManager)) != null)
                throw new ArgumentException("Graphics Device Manager Already Present");

            _game.Services.AddService(typeof(IGraphicsDeviceManager),this);
            _game.Services.AddService(typeof(IGraphicsDeviceService), this);
        }

        ~GraphicsDeviceManager()
        {
            
        }

        public void CreateDevice()
        {

        }

        public bool BeginDraw()
        {
            if (_graphicsDevice == null)
                return false;
            _drawBegun = true;
            return true;
        }

        public void EndDraw()
        {
            if(_graphicsDevice !=null && _drawBegun)
            {
                _drawBegun = false;
                _graphicsDevice.Present();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal void ResetClientBounds()
        {
#if ANDROID
#endif
        }


        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if(_graphicsDevice != null)
                    {
                        _graphicsDevice.Dispose();
                        _graphicsDevice = null;
                    }
                }
                disposed = true;
            }
        }

        #endregion


    }
}
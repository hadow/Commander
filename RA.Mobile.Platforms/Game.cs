using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RA.Mobile.Platforms.Graphics;
namespace RA.Mobile.Platforms
{
    public class Game:IDisposable
    {

#if ANDROID
        [CLSCompliant(false)]
        public static AndroidGameActivity Activity { get; internal set; }
#endif
        private bool _isDisposed;
        private static Game _instance = null;
        internal static Game Instance
        {
            get
            {
                return Game._instance;
            }
        }

        public Game()
        {
            _instance = this;
            _services = new GameServiceContainer();

            Platform = GamePlatform.PlatformCreate(this);
            Platform.Activated += OnActivated;
            Platform.Deactivated += OnDeactivated;
            _services.AddService(typeof(GamePlatform), Platform);
        }

        ~Game()
        {
            Dispose(false);
        }

        
        internal GamePlatform Platform;
        private GameServiceContainer _services;
        public GameServiceContainer Services
        {
            get
            {
                return _services;
            }
        }
        private IGraphicsDeviceService _graphicsDeviceService;

        public GraphicsDevice GraphicsDevice
        {
            get
            {
                if(_graphicsDeviceService == null)
                {
                    _graphicsDeviceService = (IGraphicsDeviceService)Services.GetService(typeof(IGraphicsDeviceService));
                    if (_graphicsDeviceService == null)
                        throw new InvalidOperationException("No Graphics Device Service");

                }
                return _graphicsDeviceService.GraphicsDevice;
            }
        }


        private IGraphicsDeviceManager _graphicsDeviceManager;

        internal GraphicsDeviceManager graphicsDeviceManager
        {
            get
            {
                if(_graphicsDeviceManager == null)
                {
                    _graphicsDeviceManager = (IGraphicsDeviceManager)Services.GetService(typeof(IGraphicsDeviceManager));
                    if (_graphicsDeviceManager == null)
                        throw new InvalidOperationException("No Graphics Device Manager");
                }
                return (GraphicsDeviceManager)_graphicsDeviceManager;
            }
        }
        public GameWindow Window
        {
            get
            {
                return Platform.Window;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sneder"></param>
        /// <param name="args"></param>
        protected virtual void OnActivated(object sneder,EventArgs args)
        {

        }

        protected virtual void OnDeactivated(object sender,EventArgs args)
        {

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="manager"></param>
        internal void ApplyChanges(GraphicsDeviceManager manager)
        {

        }


        protected virtual void Initialize()
        {
            ApplyChanges(graphicsDeviceManager);

            _graphicsDeviceService = (IGraphicsDeviceService)Services.GetService(typeof(IGraphicsDeviceService));

        }



        /// <summary>
        /// 
        /// </summary>
        internal void DoInitialize()
        {
            AssertNotDisposed();
            Platform.BeforeInitialize();

        }


        private void AssertNotDisposed()
        {
            if (_isDisposed)
            {
                string name = GetType().Name;
                throw new ObjectDisposedException(name, string.Format("The {0} object was used after being Disposed.", name));
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if(_graphicsDeviceManager != null)
                    {
                        (_graphicsDeviceManager as GraphicsDeviceManager).Dispose();
                        _graphicsDeviceManager = null;
                    }

                    if(Platform != null)
                    {
                        Platform.Activated -= OnActivated;
                        Platform.Deactivated -= OnDeactivated;
                        _services.RemoveService(typeof(GamePlatform));

                        Platform.Dispose();
                        Platform = null;
                    }
                }

#if ANDROID
                Activity = null;
#endif
                _isDisposed = true;
                _instance = null;
            }
        }
    }
}
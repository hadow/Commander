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
        private static Game _instance = null;
        internal static Game Instance
        {
            get
            {
                return Game._instance;
            }
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
        internal void DoInitialize()
        {

        }

        public void Dispose()
        {

        }
    }
}
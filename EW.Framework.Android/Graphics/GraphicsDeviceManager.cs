using System;
using Android.Views;
using EW.Framework.Mobile;
using EW.Framework.Touch;
namespace EW.Framework.Graphics
{

    /// <summary>
    /// Used by the platform code to control the graphics device.
    /// </summary>
    public interface IGraphicsDeviceManager
    {
        /// <summary>
        /// Called at the start of rendering a frame.
        /// </summary>
        /// <returns></returns>
        bool BeginDraw();

        /// <summary>
        /// Called to create the graphics device.
        /// </summary>
        void CreateDevice();

        /// <summary>
        /// Called after rendering to present the frame to the screen.
        /// </summary>
        void EndDraw();
    }


    public interface IGraphicsDeviceService
    {
        GraphicsDevice GraphicsDevice { get; }

        event EventHandler<EventArgs> DeviceCreated;
        event EventHandler<EventArgs> DeviceDisposing;
        event EventHandler<EventArgs> DeviceReset;
        event EventHandler<EventArgs> DeviceResetting;

    }
    public enum GraphicsProfile
    {
        /// <summary>
        /// Use a limited set of graphic features and capabilities, allowing the game to support the widest variety of devices.
        /// </summary>
        Reach,
        /// <summary>
        /// Use the largest available set of graphic features and capabilities to target devices, that have more enhanced graphic capabilities.        
        /// </summary>
        HiDef
    }
    public class GraphicsDeviceManager:IGraphicsDeviceService,IDisposable,IGraphicsDeviceManager
    {
        


        private Game _game;
        private GraphicsDevice _graphicsDevice;
        private int _preferredBackBufferHeight;
        private int _preferredBackBufferWidth;

        private SurfaceFormat _preferredBackBufferFormat;

        public SurfaceFormat PreferredBackBufferFormat
        {
            get { return _preferredBackBufferFormat; }
            set
            {
                _preferredBackBufferFormat = value;
            }
        }
        private DepthFormat _preferredDepthStencilFormat;

        private bool _preferMultiSampling;
        private DisplayOrientation _supportedOrientations;

        private bool _synchronizedWithVerticalRetrace = true;

        private bool _drawBegun;

        bool disposed;

        private bool _hardwareModeSwitch = true;

        private bool _wantFullScreen = false;

        public static readonly int DefaultBackBufferHeight = 480;
        public static readonly int DefaultBackBufferWidth = 800;


        public GraphicsProfile GraphicsProfile { get; set; }

        public GraphicsDevice GraphicsDevice
        {
            get { return _graphicsDevice; }
        }

        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;
        public event EventHandler<PreparingDeviceSettingsEventArgs> PreparingDeviceSettings;


        public int PreferredBackBufferHeight
        {
            get
            {
                return _preferredBackBufferHeight;
            }
            set
            {
                _preferredBackBufferHeight = value;
            }
        }

        public int PreferredBackBufferWidth
        {
            get
            {
                return _preferredBackBufferWidth;
            }
            set
            {
                _preferredBackBufferWidth = value;
            }
        }

        public DepthFormat PreferredDepthStencilFormat
        {
            get
            {
                return _preferredDepthStencilFormat;
            }
            set
            {
                _preferredDepthStencilFormat = value;
            }
        }

        public GraphicsDeviceManager(Game game)
        {
            if (game == null)
                throw new ArgumentNullException("The game cannot be null !");

            _game = game;

            _supportedOrientations = DisplayOrientation.Default;

            //Preferred buffer width/height is used to determine default supported orientations,
            //so set the default values to match XNA behaviour of landscape only  by default.
            //Note also that it's using the device window dimensions.
            _preferredBackBufferWidth = Math.Max(_game.Window.ClientBounds.Height, _game.Window.ClientBounds.Width);
            _preferredBackBufferHeight = Math.Min(_game.Window.ClientBounds.Height, _game.Window.ClientBounds.Width);

            _preferredBackBufferFormat = SurfaceFormat.Color;
            _preferredDepthStencilFormat = DepthFormat.Depth24;
            _synchronizedWithVerticalRetrace = true;

            GraphicsProfile = GraphicsProfile.Reach;

            if (_game.Services.GetService(typeof(IGraphicsDeviceManager)) != null)
                throw new ArgumentException("Graphics Device Manager Already Present");

            _game.Services.AddService(typeof(IGraphicsDeviceManager), this);
            _game.Services.AddService(typeof(IGraphicsDeviceService), this);
        }

        ~GraphicsDeviceManager()
        {
            Dispose(false);
        }

        public void CreateDevice()
        {
            Initialize();

            OnDeviceCreated(EventArgs.Empty);
        }


        internal void OnDeviceCreated(EventArgs e)
        {
            EventHelpers.Raise(this, DeviceCreated, e);
        }





        private void Initialize()
        {
            var presentationParameters = new PresentationParameters();
            presentationParameters.DepthStencilFormat = DepthFormat.Depth24;

            presentationParameters.IsFullScreen = true;

            var preparingDeviceSettingsHandler = PreparingDeviceSettings;

            if (preparingDeviceSettingsHandler != null)
            {
                GraphicsDeviceInformation gdi = new GraphicsDeviceInformation();
                gdi.GraphicsProfile = GraphicsProfile;
                gdi.Adapter = GraphicsAdapter.DefaultAdapter;
                gdi.PresentationParameters = presentationParameters;

                PreparingDeviceSettingsEventArgs pe = new PreparingDeviceSettingsEventArgs(gdi);
                preparingDeviceSettingsHandler(this, pe);
                presentationParameters = pe.GraphicsDeviceInformation.PresentationParameters;
                GraphicsProfile = pe.GraphicsDeviceInformation.GraphicsProfile;

            }

            _graphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile, presentationParameters);

            ApplyChanges();

            TouchPanel.DisplayWidth = _graphicsDevice.PresentationParameters.BackBufferWidth;
            TouchPanel.DisplayHeight = _graphicsDevice.PresentationParameters.BackBufferHeight;
            TouchPanel.DisplayOrientation = _graphicsDevice.PresentationParameters.DisplayOrientation;
        }



        public bool SynchronizeWithVerticalRetrace
        {
            get { return _synchronizedWithVerticalRetrace; }
            set
            {
                _synchronizedWithVerticalRetrace = value;
            }
        }

        public bool IsFullScreen
        {
            get
            {
                if (_graphicsDevice != null)
                    return _graphicsDevice.PresentationParameters.IsFullScreen;
                return _wantFullScreen;
            }
            set
            {
                _wantFullScreen = value;

                if(_graphicsDevice != null)
                {
                    _graphicsDevice.PresentationParameters.IsFullScreen = value;

                    ForceSetFullScreen();
                }
            }
        }

        public DisplayOrientation SupportedOrientations
        {
            get
            {
                return _supportedOrientations;
            }
            set
            {
                _supportedOrientations = value;
                if (_game.Window != null)
                    _game.Window.SetSupportedOrientations(_supportedOrientations);
            }
        }


        internal void ForceSetFullScreen()
        {
            if (IsFullScreen)
            {
                Game.Activity.Window.ClearFlags(WindowManagerFlags.ForceNotFullscreen);
                Game.Activity.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            }
            else
            {
                Game.Activity.Window.SetFlags(WindowManagerFlags.ForceNotFullscreen, WindowManagerFlags.ForceNotFullscreen);
            }
        }


        public void ApplyChanges()
        {
            //Calling ApplyChange() before CreateDevice() should have no effect.
            if (_graphicsDevice == null)
                return;

            //Trigger a change in orientation in case the supported orientations have changed
            ((AndroidGameWindow)_game.Window).SetOrientation(_game.Window.CurrentOrientation, false);

            //Ensure the presentation parameter orientation and buffer size matches the window
            _graphicsDevice.PresentationParameters.DisplayOrientation = _game.Window.CurrentOrientation;

            //Set the presentation parameters' actual buffer size to match the orientation

            bool isLandscape = (0 != (_game.Window.CurrentOrientation & (DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight)));

            int w = _preferredBackBufferWidth;
            int h = _preferredBackBufferHeight;

            _graphicsDevice.PresentationParameters.BackBufferWidth = isLandscape ? Math.Max(w, h) : Math.Min(w, h);
            _graphicsDevice.PresentationParameters.BackBufferHeight = isLandscape ? Math.Min(w, h) : Math.Max(w, h);

            ResetClientBounds();

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

            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This method is used by MonoGame Android to adjust the game's drawn to area to fill
        /// as much of the screen as possible whilst retaining the aspect ratio inferred from
        /// aspectRatio = (PreferredBackBufferWidth / PreferredBackBufferHeight)
        ///
        /// NOTE: this is a hack that should be removed if proper back buffer to screen scaling
        /// is implemented. To disable it's effect, in the game's constructor use:
        ///
        ///     graphics.IsFullScreen = true;
        ///     graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
        ///     graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
        ///
        /// </summary>
        internal void ResetClientBounds()
        {
#if ANDROID
            float preferredAspectRatio = (float)PreferredBackBufferWidth /
                                         (float)PreferredBackBufferHeight;
            float displayAspectRatio = (float)GraphicsDevice.DisplayMode.Width /
                                       (float)GraphicsDevice.DisplayMode.Height;

            float adjustedAspectRatio = preferredAspectRatio;

            if ((preferredAspectRatio > 1.0f && displayAspectRatio < 1.0f) ||
                (preferredAspectRatio < 1.0f && displayAspectRatio > 1.0f))
            {
                // Invert preferred aspect ratio if it's orientation differs from the display mode orientation.
                // This occurs when user sets preferredBackBufferWidth/Height and also allows multiple supported orientations
                adjustedAspectRatio = 1.0f / preferredAspectRatio;
            }

            const float EPSILON = 0.00001f;
            var newClientBounds = new Rectangle();
            if (displayAspectRatio > (adjustedAspectRatio + EPSILON))
            {
                // Fill the entire height and reduce the width to keep aspect ratio
                newClientBounds.Height = _graphicsDevice.DisplayMode.Height;
                newClientBounds.Width = (int)(newClientBounds.Height * adjustedAspectRatio);
                newClientBounds.X = (_graphicsDevice.DisplayMode.Width - newClientBounds.Width) / 2;
            }
            else if (displayAspectRatio < (adjustedAspectRatio - EPSILON))
            {
                // Fill the entire width and reduce the height to keep aspect ratio
                newClientBounds.Width = _graphicsDevice.DisplayMode.Width;
                newClientBounds.Height = (int)(newClientBounds.Width / adjustedAspectRatio);
                newClientBounds.Y = (_graphicsDevice.DisplayMode.Height - newClientBounds.Height) / 2;
            }
            else
            {
                // Set the ClientBounds to match the DisplayMode
                newClientBounds.Width = GraphicsDevice.DisplayMode.Width;
                newClientBounds.Height = GraphicsDevice.DisplayMode.Height;
            }

            // Ensure buffer size is reported correctly
            _graphicsDevice.PresentationParameters.BackBufferWidth = newClientBounds.Width;
            _graphicsDevice.PresentationParameters.BackBufferHeight = newClientBounds.Height;

            // Set the veiwport so the (potentially) resized client bounds are drawn in the middle of the screen
            _graphicsDevice.Viewport = new Viewport(newClientBounds.X, -newClientBounds.Y, newClientBounds.Width, newClientBounds.Height);

            ((AndroidGameWindow)_game.Window).ChangeClientBounds(newClientBounds);

            // Touch panel needs latest buffer size for scaling
            TouchPanel.DisplayWidth = newClientBounds.Width;
            TouchPanel.DisplayHeight = newClientBounds.Height;

            //Android.Util.Log.Debug("MonoGame", "GraphicsDeviceManager.ResetClientBounds: newClientBounds=" + newClientBounds.ToString());
#endif
        }


        protected virtual void Dispose(bool disposing)
        {

            if (!disposed)
            {
                if (disposing)
                {
                    if(_graphicsDevice != null)
                    {
                        _graphicsDevice = null;
                    }
                }
                disposed = true;
            }
        }




    }
}
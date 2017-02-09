using System;
using System.Collections;
using System.Collections.Generic;
using OpenTK.Platform.Android;
using OpenTK.Graphics;
using Android.Views;
using Android.Content;
using Android.Graphics;
using Android.OS;
using RA.Mobile.Platforms.Graphics;
using RA.Mobile.Platforms.Content;
namespace RA.Mobile.Platforms
{
    /// <summary>
    /// 游戏视图
    /// </summary>
	public class RAndroidGameView:AndroidGameView,View.IOnTouchListener,ISurfaceHolderCallback
	{

        private readonly Game _game;

		private readonly AndroidGameWindow _gameWindow;
		private readonly AndroidTouchEventManager _touchManager;

		public bool isResuming { get; private set; }

		private bool _lostContext;
		private bool _backPressed;

        public bool TouchEnabled
        {
            get
            {
                return _touchManager.Enabled;
            }
            set
            {
                _touchManager.Enabled = value;
                SetOnTouchListener(value ? this : null);
            }
        }
		public RAndroidGameView(Context context,AndroidGameWindow window,Game game):base(context)
		{
			_touchManager = new AndroidTouchEventManager(window);
			_gameWindow = window;
            _game = game;
		}

#region IOnTouchListener implementation
		bool IOnTouchListener.OnTouch(View v, MotionEvent evt)
		{
            _touchManager.OnTouchEvent(evt);
			return true;
		}
        #endregion

        


        #region ISurfaceHolderCallback implementation

        private bool _isSurfaceChanged = false;
        private int _prevSurfaceWidth = 0;
        private int _prevSurfaceHeight = 0;

        void ISurfaceHolderCallback.SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height)
        {
            if ((int)Build.VERSION.SdkInt >= 19)
            {
                if (!_isSurfaceChanged)
                {
                    _isSurfaceChanged = true;
                    _prevSurfaceWidth = width;
                    _prevSurfaceHeight = height;
                }
                else
                {
                    if(!ScreenReciever.ScreenLocked && Game.Instance.Platform.IsActive &&(_prevSurfaceHeight != height || _prevSurfaceWidth!= width))
                    {
                        _prevSurfaceWidth = width;
                        _prevSurfaceHeight = height;

                        base.SurfaceDestroyed(holder);
                        base.SurfaceCreated(holder);
                    }
                }
            }


            if (width < height && (_game.graphicsDeviceManager.SupportedOrientations & DisplayOrientation.Portrait) == 0)
                return;

            var manager = _game.graphicsDeviceManager;
            
            manager.PreferredBackBufferHeight = height;
            manager.PreferredBackBufferWidth = width;

            if (manager.GraphicsDevice != null)
                manager.GraphicsDevice.Viewport = new Viewport(0, 0, width, height);

            _gameWindow.ChangeClientBounds(new Rectangle(0, 0, width, height));
            manager.ApplyChanges();

            SurfaceChanged(holder, format, width, height);

            if (_game.GraphicsDevice != null)
                _game.graphicsDeviceManager.ResetClientBounds();
        }

        void ISurfaceHolderCallback.SurfaceDestroyed(ISurfaceHolder holder)
        {
            SurfaceDestroyed(holder);
            
        }

        void ISurfaceHolderCallback.SurfaceCreated(ISurfaceHolder holder)
        {
            SurfaceCreated(holder);

            _isSurfaceChanged = false;
        }
        #endregion

        #region AndroidGameView

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                MakeCurrent();
            }
            catch(Exception exp)
            {
                
            }
        }

        public override void Resume()
        {
            if (!ScreenReciever.ScreenLocked && Game.Instance.Platform.IsActive)
                base.Resume();
        }


        protected override void OnContextSet(EventArgs e)
        {
            base.OnContextSet(e);

            if (_lostContext)
            {
                _lostContext = true;
                if (_game.GraphicsDevice != null)
                {
                    _game.GraphicsDevice.Initialize();
                    isResuming = true;
                    if (_gameWindow.Resumer != null)
                    {
                        _gameWindow.Resumer.LoadContent();
                    }

                    System.Threading.Thread bgThread = new System.Threading.Thread(o => {

                        ContentManager.ReloadGraphicsContent();

                        _game.graphicsDeviceManager.OnDeviceReset(EventArgs.Empty);
                        _game.GraphicsDevice.OnDeviceResetting();

                        isResuming = false;
                    });

                    bgThread.Start();
                }

            }
        }



        protected override void OnContextLost(EventArgs e)
        {
            base.OnContextLost(e);
            _game.graphicsDeviceManager.OnDeviceReseting(EventArgs.Empty);
            if (_game.GraphicsDevice != null)
                _game.GraphicsDevice.OnDeviceResetting();

            _lostContext = true;
        }


        protected override void CreateFrameBuffer()
        {
            int depth = 0;
            int stencil = 0;

            switch (_game.graphicsDeviceManager.PreferredDepthStencilFormat)
            {
                case DepthFormat.Depth16:
                    depth = 16;
                    break;
                case DepthFormat.Depth24:
                    depth = 24;
                    break;
                case DepthFormat.Depth24Stencil8:
                    depth = 24;
                    stencil = 8;
                    break;
                case DepthFormat.None:
                    break;
            }

            List<GraphicsMode> modes = new List<GraphicsMode>();
            if (depth > 0)
            {
                modes.Add(new AndroidGraphicsMode(new ColorFormat(8, 8, 8, 8), depth, stencil, 0, 0, false));
                modes.Add(new AndroidGraphicsMode(new ColorFormat(5, 6, 5, 0), depth, stencil, 0, 0, false));
                modes.Add(new AndroidGraphicsMode(0, depth, stencil, 0, 0, false));
                if (depth > 16)
                {
                    modes.Add(new AndroidGraphicsMode(new ColorFormat(8, 8, 8, 8), 16, 0, 0, 0, false));
                    modes.Add(new AndroidGraphicsMode(new ColorFormat(5, 6, 5, 0), 16, 0, 0, 0, false));
                    modes.Add(new AndroidGraphicsMode(0, 16, 0, 0, 0, false));
                }
            }
            else
            {
                modes.Add(new AndroidGraphicsMode(new ColorFormat(8, 8, 8, 8), depth, stencil, 0, 0, false));
                modes.Add(new AndroidGraphicsMode(new ColorFormat(5, 6, 5, 0), depth, stencil, 0, 0, false));
            }

            modes.Add(null);//default mode
            modes.Add(new AndroidGraphicsMode(0, 0, 0, 0, 0, false));//low mode

            Exception innerException = null;
            foreach(GraphicsMode mode in modes)
            {
                if (mode != null)
                {

                }
                else
                {

                }
                GraphicsMode = mode;
                try
                {
                    base.CreateFrameBuffer();
                }
                catch(Exception e)
                {
                    innerException = e;
                    continue;
                }
                var status = OpenTK.Graphics.ES20.GL.CheckFramebufferStatus(OpenTK.Graphics.ES20.FramebufferTarget.Framebuffer);
                MakeCurrent();
                return;
            }
                
                
        }



        #endregion

    }
}

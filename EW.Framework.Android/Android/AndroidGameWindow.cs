using System;
using Android.Content;
using Android.Content.PM;
using Android.Views;
using Android.OS;
using EW.Framework.Touch;
using EW.Framework.Graphics;

namespace EW.Framework.Mobile
{
    /// <summary>
    /// Android Platform游戏视窗口
    /// </summary>
    /// 
    [CLSCompliant(false)]
	public class AndroidGameWindow : GameWindow, IDisposable
	{
        internal AndroidGameView GameView { get; private set; }

        private readonly Game _game;

		private Rectangle _clientBounds;

		private DisplayOrientation _supportedOrientations = DisplayOrientation.Default;
		private DisplayOrientation _currentOrientation;

		public AndroidGameWindow(AndroidGameActivity activity,Game game)
		{
            _game = game;

            EW.Framework.Point size;
            if (Build.VERSION.SdkInt < BuildVersionCodes.JellyBean)
            {
                size.X = activity.Resources.DisplayMetrics.WidthPixels;
                size.Y = activity.Resources.DisplayMetrics.HeightPixels;
            }
            else
            {
                
                
                Android.Graphics.Point p = new Android.Graphics.Point();
                activity.WindowManager.DefaultDisplay.GetRealSize(p);
                size.X = p.X;
                size.Y = p.Y;
            }
            Initialize(activity);

            game.Services.AddService(typeof(View), GameView);
		}
        

        /// <summary>
        /// 初始化上下文
        /// </summary>
        /// <param name="context"></param>
        private void Initialize(Context context)
        {

            _clientBounds = new Rectangle(0, 0, context.Resources.DisplayMetrics.WidthPixels, context.Resources.DisplayMetrics.HeightPixels);

            GameView = new AndroidGameView(context, this,_game);
            GameView.LogFPS = true;
            GameView.RenderOnUIThread = Game.Activity.RenderOnUIThread;
            GameView.RenderFrame += OnRenderFrame;
            GameView.UpdateFrame += OnUpdateFrame;

            GameView.RequestFocus();
            GameView.FocusableInTouchMode = true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="frameEventArgs"></param>
        private void OnUpdateFrame(object sender,AndroidGameView.FrameEventArgs frameEventArgs)
        {
            GameView.MakeCurrent();

            Threading.Run();

            if(_game != null)
            {
                if(!GameView.IsResuming && _game.Platform.IsActive && !ScreenReciever.ScreenLocked) //only call draw if an update has occured
                {
                    _game.Tick();
                }
                else if(_game.GraphicsDevice != null)
                {
                    _game.GraphicsDevice.Clear(System.Drawing.Color.Black);

                    _game.Platform.Present();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="frameEventArgs"></param>
        private void OnRenderFrame(object sender,AndroidGameView.FrameEventArgs frameEventArgs)
        {
            GameView.MakeCurrent();

            Threading.Run();
        }

		internal protected override void SetSupportedOrientations(DisplayOrientation orientations)
		{
			_supportedOrientations = orientations;
		}

		public override IntPtr Handle
		{
			get
			{
				return IntPtr.Zero;
			}
		}
	
		public override DisplayOrientation CurrentOrientation
		{
			get
			{
				return _currentOrientation;
			}
		}


		public override string ScreenDeviceName
		{
			get
			{
				throw new NotImplementedException();
			}
				
		}


		public override Rectangle ClientBounds
		{
			get
			{
				return _clientBounds;
			}
		}

		public override bool AllowUserResizing
		{
			get
			{
				return false;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		protected override void SetTitle(string title)
		{
        }


		public override void BeginScreenDeviceChange(bool willBeFullScreen)
		{
		}


		public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
		{
		}

        /// <summary>
        /// Updates the screen orientation,Filters out requests for unsupported orientations.
        /// </summary>
        /// <param name="newOrientation"></param>
        internal void SetOrientation(DisplayOrientation newOrientation,bool applyGraphicsChanges)
        {
            DisplayOrientation supported = GetEffectiveSupportedOrientations();

            if((supported & newOrientation) == 0)
            {
                if ((supported & DisplayOrientation.LandscapeLeft) != 0)
                    newOrientation = DisplayOrientation.LandscapeLeft;
                else if ((supported & DisplayOrientation.LandscapeRight) != 0)
                    newOrientation = DisplayOrientation.LandscapeRight;
                else if ((supported & DisplayOrientation.Portrait) != 0)
                    newOrientation = DisplayOrientation.Portrait;
                else if ((supported & DisplayOrientation.PortraitDown) != 0)
                    newOrientation = DisplayOrientation.PortraitDown;
            }

            DisplayOrientation oldOrientation = CurrentOrientation;

            SetDisplayOrientation(newOrientation);
            TouchPanel.DisplayOrientation = newOrientation;

            if (applyGraphicsChanges) {

                _game.graphicsDeviceManager.ApplyChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void SetDisplayOrientation(DisplayOrientation value)
        {
            if (value != _currentOrientation)
            {
                DisplayOrientation supported = GetEffectiveSupportedOrientations();
                ScreenOrientation requestedOrientation = ScreenOrientation.Unspecified;

                bool wasPortrait = _currentOrientation == DisplayOrientation.Portrait || _currentOrientation == DisplayOrientation.PortraitDown;
                bool requestPortrait = false;

                bool didOrientationChange = false;

                //Android 2.3 and above support reverse orientations
                int sdkVer = (int)Android.OS.Build.VERSION.SdkInt;
                if (sdkVer >= 10)
                {
                    if((supported & value) != 0)
                    {
                        didOrientationChange = true;
                        _currentOrientation = value;
                        switch (value)
                        {

                        }
                    }
                }
                else
                {

                }

                if (didOrientationChange)
                {
                    if(wasPortrait != requestPortrait)
                    {
                        TouchPanelState.ReleaseAllTouches();
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="bounds"></param>
        internal void ChangeClientBounds(Rectangle bounds)
        {
            if(bounds != _clientBounds)
            {
                _clientBounds = bounds;
                OnClientSizeChanged();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal DisplayOrientation GetEffectiveSupportedOrientations()
        {
            if(_supportedOrientations == DisplayOrientation.Default)
            {
                var deviceManager = (_game.Services.GetService(typeof(IGraphicsDeviceManager)) as GraphicsDeviceManager);
                if (deviceManager == null)
                    return DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

                if(deviceManager.PreferredBackBufferWidth > deviceManager.PreferredBackBufferHeight)
                {
                    return DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
                }
                else
                {
                    return DisplayOrientation.Portrait | DisplayOrientation.PortraitDown;
                }
            }
            else
            {
                return _supportedOrientations;
            }


        }
		public void Dispose()
		{
            if (GameView != null)
            {
                GameView.Dispose();
                GameView = null;
            }


		}



	}
}

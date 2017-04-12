using System;
using Android.Content;
using Android.Views;
using EW.Xna.Platforms.Input.Touch;
using OpenTK;
namespace EW.Xna.Platforms
{
    /// <summary>
    /// Android Platform游戏视窗口
    /// </summary>
	public class AndroidGameWindow : GameWindow, IDisposable
	{
        internal RAndroidGameView GameView { get; private set; }
        internal IResumeManager Resumer;

        private readonly Game _game;

		private Rectangle _clientBounds;

		private DisplayOrientation _supportedOrientations = DisplayOrientation.Default;
		private DisplayOrientation _currentOrientation;

		public AndroidGameWindow(AndroidGameActivity activity,Game game)
		{
            _game = game;
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

            GameView = new RAndroidGameView(context, this,_game);
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
        private void OnUpdateFrame(object sender,FrameEventArgs frameEventArgs)
        {

            if (!GameView.GraphicsContext.IsCurrent)
                GameView.MakeCurrent();

            Threading.Run();

            if(_game != null)
            {
                if(!GameView.isResuming && _game.Platform.IsActive && !ScreenReciever.ScreenLocked)
                {
                    _game.Tick();
                }
                else if(_game.GraphicsDevice != null)
                {
                    _game.GraphicsDevice.Clear(Color.Black);
                    if(GameView.isResuming && Resumer != null)
                    {
                        Resumer.Draw();
                    }
                    _game.Platform.Present();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="frameEventArgs"></param>
        private void OnRenderFrame(object sender,FrameEventArgs frameEventArgs)
        {
            if (GameView.GraphicsContext == null || GameView.GraphicsContext.IsDisposed)
                return;
            if (!GameView.GraphicsContext.IsCurrent)
                GameView.MakeCurrent();
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
			throw new NotImplementedException();
		}


		public override void BeginScreenDeviceChange(bool willBeFullScreen)
		{
			throw new NotImplementedException();
		}


		public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
		{
			throw new NotImplementedException();
		}

        /// <summary>
        /// 
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

            if (applyGraphicsChanges)
                _game.graphicsDeviceManager.ApplyChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void SetDisplayOrientation(DisplayOrientation value)
        {
            if (value != _currentOrientation)
            {

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
            return _supportedOrientations;
        }
		public void Dispose()
		{



		}



	}
}

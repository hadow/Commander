using System;
using System.Drawing;
using Android.Content;
using OpenTK;
namespace RA.Mobile.Platforms
{
	public class AndroidGameWindow : GameWindow, IDisposable
	{

        internal RAndroidGameView GameView { get; private set; }

        private readonly Game _game;

		private Rectangle _clientBounds;

		private DisplayOrientation _supportedOrientations = DisplayOrientation.Default;
		private DisplayOrientation _currentOrientation;
		public AndroidGameWindow()
		{
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

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="frameEventArgs"></param>
        private void OnUpdateFrame(object sender,FrameEventArgs frameEventArgs)
        {

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


        }
		public void Dispose()
		{



		}



	}
}

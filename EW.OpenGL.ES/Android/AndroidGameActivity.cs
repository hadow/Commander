using System;
using Android.App;
using Android.Content;
using Android.OS;
namespace EW.OpenGLES.Mobile
{
    /// <summary>
    /// 
    /// </summary>
    /// 
    [CLSCompliant(false)]
	public class AndroidGameActivity:Activity
	{
        internal Game Game { private get; set; }

		public AndroidGameActivity()
		{
		}

        public bool RenderOnUIThread = true;
		private ScreenReciever _screenReciever;
		private OrientationListener _orientationListener;

		public static event EventHandler Paused;
		public static event EventHandler Resumed;
        /// <summary>
        /// OnCreate called when the activity is launched from cold or after the app
        /// has been killed due to a higher priority app needing the memory
        /// </summary>
        /// <param name="savedInstanceState"></param>
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			IntentFilter filter = new IntentFilter();
			filter.AddAction(Intent.ActionScreenOn);
			filter.AddAction(Intent.ActionScreenOff);
			filter.AddAction(Intent.ActionUserPresent);

			_screenReciever = new ScreenReciever();
			RegisterReceiver(_screenReciever, filter);

			_orientationListener = new OrientationListener(this);
            Game.Activity = this;
		}


        /// <summary>
        /// 
        /// </summary>
		protected override void OnPause()
		{
			base.OnPause();
            if (Paused != null)
                Paused(this, EventArgs.Empty);

            if (_orientationListener.CanDetectOrientation())
                _orientationListener.Disable();

		}

        /// <summary>
        /// 复原
        /// </summary>
		protected override void OnResume()
		{
			base.OnResume();
            if (Resumed != null)
                Resumed(this, EventArgs.Empty);

            if (Game != null)
            {
                ((AndroidGameWindow)Game.Window).GameView.RequestFocus();
                if (_orientationListener.CanDetectOrientation())
                    _orientationListener.Enable();
            }
		}


        /// <summary>
        /// 销毁
        /// </summary>

		protected override void OnDestroy()
		{
            UnregisterReceiver(_screenReciever);
            ScreenReciever.ScreenLocked = false;
            _orientationListener = null;
            if (Game != null)
                Game.Dispose();
            Game = null;
			base.OnDestroy();
		}
	}
}

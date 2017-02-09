using System;
using Android.App;
using Android.Content;
using Android.OS;
namespace RA.Mobile.Platforms
{
    /// <summary>
    /// 
    /// </summary>
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


		protected override void OnPause()
		{
			base.OnPause();
		}


		protected override void OnResume()
		{
			base.OnResume();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
		}
	}
}

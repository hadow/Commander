using System;
using OpenTK.Platform.Android;
using Android.Views;
using Android.Content;
namespace RA.Mobile.Platforms
{
	public class RAndroidGameView:AndroidGameView,View.IOnTouchListener,ISurfaceHolderCallback
	{
		public RAndroidGameView(Context context,AndroidGameWindow window):base(context)
		{

			_touchManager = new AndroidTouchEventManager();
			_gameWindow = window;


		}


		private readonly AndroidGameWindow _gameWindow;
		private readonly AndroidTouchEventManager _touchManager;

		public bool isResuming { get; private set; }

		private bool _lostContext;
		private bool _backPressed;


#region IOnTouchListener implementation
		bool IOnTouchListener.OnTouch(View v, MotionEvent evt)
		{
			return true;
		}





	}
}

using System;
using OpenTK.Platform.Android;
using Android.Views;
using Android.Content;
namespace RA.Mobile.Platforms
{
	public class RAndroidGameView:AndroidGameView,View.IOnTouchListener,ISurfaceHolderCallback
	{

        private readonly Game _game;

		public RAndroidGameView(Context context,AndroidGameWindow window,Game game):base(context)
		{
			_touchManager = new AndroidTouchEventManager();
			_gameWindow = window;
            _game = game;
            
		}


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

#region IOnTouchListener implementation
		bool IOnTouchListener.OnTouch(View v, MotionEvent evt)
		{
			return true;
		}

        public override void Resume()
        {
            if(!ScreenReciever.ScreenLocked && Game.Instance.Platform.IsActive)
                base.Resume();
        }




    }
}

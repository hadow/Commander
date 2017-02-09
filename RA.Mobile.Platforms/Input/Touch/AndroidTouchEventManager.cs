using System;
using Android.Views;
using RA.Mobile.Platforms.Input.Touch;
namespace RA.Mobile.Platforms
{
    /// <summary>
    /// 
    /// </summary>
	public class AndroidTouchEventManager
	{
        readonly AndroidGameWindow _gameWindow;

        public bool Enabled { get; set; }

		public AndroidTouchEventManager(AndroidGameWindow androidGameWindow)
		{
            _gameWindow = androidGameWindow;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        public void OnTouchEvent(MotionEvent evt)
        {
            if (!Enabled)
                return;

            Vector2 position = Vector2.Zero;
            position.X = evt.GetX(evt.ActionIndex);
            position.Y = evt.GetY(evt.ActionIndex);
            UpdateTouchPosition(ref position);
            int id = evt.GetPointerId(evt.ActionIndex);
            switch (evt.ActionMasked)
            {
                case MotionEventActions.Down:
                case MotionEventActions.PointerDown:
                    TouchPanel.AddEvent(id, TouchLocationState.Pressed, position);
                    break;
                case MotionEventActions.Up:
                case MotionEventActions.PointerUp:
                    TouchPanel.AddEvent(id, TouchLocationState.Released, position);
                    break;
                case MotionEventActions.Move:
                    for(int i = 0; i < evt.PointerCount; i++)
                    {
                        id = evt.GetPointerId(i);
                        position.X = evt.GetX(i);
                        position.Y = evt.GetY(i);
                        UpdateTouchPosition(ref position);
                        TouchPanel.AddEvent(id, TouchLocationState.Moved, position);
                    }
                    break;
                case MotionEventActions.Cancel:
                case MotionEventActions.Outside:
                    for(int i = 0; i < evt.PointerCount; i++)
                    {
                        id = evt.GetPointerId(i);
                        TouchPanel.AddEvent(id, TouchLocationState.Released, position);
                    }
                    break;
            }

        }

        void UpdateTouchPosition(ref Vector2 position)
        {
            Rectangle clientBounds = _gameWindow.ClientBounds;
            position.X -= clientBounds.X;
            position.Y -= clientBounds.Y;
        }

	}
}

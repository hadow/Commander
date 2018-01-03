using System;
using System.Collections.Generic;
using System.Drawing;
namespace EW.Framework.Touch
{


    [Flags]
    public enum GestureT
    {
        None = 0,
        Tap = 1,
        DragComplete = 2,
        Flick = 4,          //Çá·÷¶ø¹ý
        FreeDrag = 8,
        Hold = 16,
        HorizontalDrag = 32,
        Pinch = 64,
        PinchComplete = 128,
        DoubleTap = 256,
        VerticalDrag = 512,
    }



    /// <summary>
    /// ´¥¿ØÃæ°å×´Ì¬
    /// </summary>
    public class TouchPanelState
    {


        /// <summary>
        /// The current size of the display
        /// </summary>
        private Point _displaySize = Point.Zero;

        private Vector2 _touchScale = Vector2.One;
        /// <summary>
        /// touch ids -> touch locations
        /// </summary>
        private readonly Dictionary<int, int> _touchIds = new Dictionary<int, int>();

        public GestureT EnabledGestrues { get; set; }


        public bool EnableMouseTouchPoint { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool EnableMouseGestrues { get; set; }


        public int DisplayHeight
        {
            get { return _displaySize.Y; }
            set
            {
                _displaySize.Y = value;

            }
        }

        public int DisplayWidth
        {
            get { return _displaySize.X; }
            set { _displaySize.X = value; }
        }

        private const int MouseTouchId = 1;

        /// <summary>
        /// The next touch location identifier.
        /// </summary>
        private int _nextTouchId = 2;
        internal static TimeSpan CurrentTimestamp { get; set; }//µ±Ç°•rég´Á

        internal readonly GameWindow Window;
        public TouchPanelState(GameWindow window)
        {
            Window = window;
        }

        /// <summary>
        /// 
        /// </summary>
        public DisplayOrientation DisplayOrientation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <param name="position"></param>
        internal void AddEvent(int id, TouchLocationState state, Vector2 position)
        {
            AddEvent(id, state, position, false);
        }


        internal void AddEvent(int id, TouchLocationState state, Vector2 position, bool isMouse)
        {
            if(state == TouchLocationState.Pressed)
            {
                if (isMouse)
                {
                    _touchIds[id] = MouseTouchId;
                }
                else
                {
                    _touchIds[id] = _nextTouchId++;
                }
            }

            int touchId;
            if(!_touchIds.TryGetValue(id,out touchId))
            {
                return;
            }

            if(!isMouse || EnableMouseGestrues || EnableMouseTouchPoint)
            {
                var evt = new TouchLocation(touchId, state, position * _touchScale, CurrentTimestamp);
                if(!isMouse || EnableMouseTouchPoint)
                {

                }
            }
        }

        internal void ReleaseAllTouches()
        {

        }
    }
}
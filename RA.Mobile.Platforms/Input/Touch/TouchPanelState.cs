using System;
using System.Collections.Generic;

namespace RA.Mobile.Platforms.Input.Touch
{

    /// <summary>
    /// 
    /// </summary>
    public class TouchPanelState
    {

        private const int MouseTouchId = 1;
        internal static TimeSpan CurrentTimestamp { get; set; }//µ±Ç°•rég´Á
        public TouchPanelState(GameWindow window)
        {

        }

        private readonly Dictionary<int, int> _touchIds = new Dictionary<int, int>();

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
        }


        internal void AddEvent(int id, TouchLocationState state, Vector2 position, bool isMouse)
        {

        }
    }
}
using System;
using System.Collections.Generic;

namespace RA.Mobile.Platforms.Input.Touch
{
    public class TouchPanelState
    {

        private const int MouseTouchId = 1;

        public TouchPanelState(GameWindow window)
        {

        }

        private readonly Dictionary<int, int> _touchIds = new Dictionary<int, int>();

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
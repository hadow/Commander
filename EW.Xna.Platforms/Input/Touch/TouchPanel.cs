using System;
using System.Collections.Generic;

namespace EW.Xna.Platforms.Input.Touch
{
    /// <summary>
    /// ´¥¿Ø²Ù×÷Ãæ°å
    /// </summary>
    public static class TouchPanel
    {
        internal static GameWindow PrimaryWindow;


        public static int DisplayWidth
        {
            get { return PrimaryWindow.TouchPanelState.DisplayWidth; }
            set { PrimaryWindow.TouchPanelState.DisplayWidth = value; }
        }

        public static int DisplayHeight
        {
            get { return PrimaryWindow.TouchPanelState.DisplayHeight; }
            set { PrimaryWindow.TouchPanelState.DisplayHeight = value; }
        }


        
        internal static void AddEvent(int id,TouchLocationState state,Vector2 position)
        {
            AddEvent(id, state, position, false);
        }

        internal static void AddEvent(int id,TouchLocationState state,Vector2 position,bool isMouse)
        {
            PrimaryWindow.TouchPanelState.AddEvent(id, state, position, isMouse);
        }

        public static DisplayOrientation DisplayOrientation
        {
            get { return PrimaryWindow.TouchPanelState.DisplayOrientation; }
            set
            {
                PrimaryWindow.TouchPanelState.DisplayOrientation = value;
            }
        }



    }
}
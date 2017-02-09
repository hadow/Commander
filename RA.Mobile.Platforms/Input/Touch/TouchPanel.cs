using System;
using System.Collections.Generic;

namespace RA.Mobile.Platforms.Input.Touch
{
    /// <summary>
    /// 
    /// </summary>
    public static class TouchPanel
    {
        internal static GameWindow PrimaryWindow;


        
        internal static void AddEvent(int id,TouchLocationState state,Vector2 position)
        {
            AddEvent(id, state, position, false);
        }

        internal static void AddEvent(int id,TouchLocationState state,Vector2 position,bool isMouse)
        {
            PrimaryWindow.TouchPanelState.AddEvent(id, state, position, isMouse);
        }





    }
}
using System;
using Android.Views;
using Android.App;
using Android.Content;
namespace EW.Xna.Platforms
{
    /// <summary>
    /// 
    /// </summary>
	public class ScreenReciever:BroadcastReceiver
	{
        public static bool ScreenLocked;
		public ScreenReciever()
		{
		}


        public override void OnReceive(Context context, Intent intent)
        {
            if(intent.Action == Intent.ActionScreenOn)
            {
                KeyguardManager keyguard = (KeyguardManager)context.GetSystemService(Context.KeyguardService);
                if (!keyguard.InKeyguardRestrictedInputMode())
                    OnUnlocked();
            }
            else if(intent.Action == Intent.ActionScreenOff)
            {
                OnLocked();
            }
            else if(intent.Action == Intent.ActionUserPresent)
            {
                OnUnlocked();
            }
        }

        private void OnLocked()
        {
            ScreenReciever.ScreenLocked = true;
        }

        private void OnUnlocked()
        {
            ScreenReciever.ScreenLocked = false;
            ((AndroidGameWindow)Game.Instance.Window).GameView.Resume();
        }

    }
}

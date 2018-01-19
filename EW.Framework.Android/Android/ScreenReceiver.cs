using Android.App;
using Android.Content;
using Android.Util;
namespace EW.Framework.Mobile
{
    /// <summary>
    /// 
    /// </summary>
	public class ScreenReceiver:BroadcastReceiver
	{
        public static bool ScreenLocked;
        
        public override void OnReceive(Context context, Intent intent)
        {
            Log.Info("AndroidGame", intent.Action.ToString());
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
            ScreenReceiver.ScreenLocked = true;
        }

        private void OnUnlocked()
        {
            ScreenReceiver.ScreenLocked = false;
            ((AndroidGameWindow)Game.Instance.Window).GameView.Resume();
        }

    }
}

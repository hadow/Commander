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
            throw new NotImplementedException();
        }



    }
}

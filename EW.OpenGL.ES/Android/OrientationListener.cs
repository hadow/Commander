using System;
using Android.Views;
using Android.Content;
using Android.Hardware;
namespace EW.OpenGLES.Mobile
{
    /// <summary>
    /// 设备自适应朝向监听
    /// </summary>
	internal class OrientationListener:OrientationEventListener
	{
		public OrientationListener(Context context) : base(context, SensorDelay.Ui) { }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="orientation"></param>
        public override void OnOrientationChanged(int orientation)
        {
            if (orientation == OrientationEventListener.OrientationUnknown)
                return;
            if (ScreenReciever.ScreenLocked)
                return;

            var disporientation = AndroidCompatibility.GetAbsoluteOrientation(orientation);

            AndroidGameWindow gameWindow = (AndroidGameWindow)Game.Instance.Window;
            if((gameWindow.GetEffectiveSupportedOrientations() & disporientation)!=0 && disporientation != gameWindow.CurrentOrientation)
            {
                gameWindow.SetOrientation(disporientation, true);
            }

        }
    }
}

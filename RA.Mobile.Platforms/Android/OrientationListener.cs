using System;r
using Android.Views;
using Android.Content;
using Android.Hardware;
namespace RA.Mobile.Platforms
{
	public class OrientationListener:OrientationListener
	{
		public OrientationListener(Context context):base(context,SensorDelay.Ui)
		{
			
		}
	}
}

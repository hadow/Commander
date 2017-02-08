using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Views;

namespace RA.Mobile.Platforms.Android
{
    public static class AndroidCompatibility
    {
        internal static DisplayOrientation GetAbsoluteOrientation(int orientation)
        {
            var disporientation = DisplayOrientation.Unknown;
            return disporientation;
        }

        public static DisplayOrientation GetAbsoluteOrientation()
        {
            var orientation = Game.Activity.WindowManager.DefaultDisplay.Rotation;


            int degrees;
            switch (orientation)
            {
                case SurfaceOrientation.Rotation90:
                    degrees = 270;
                    break;
                case SurfaceOrientation.Rotation180:
                    degrees = 180;
                    break;
                case SurfaceOrientation.Rotation270:
                    degrees = 90;
                    break;
                default:
                    degrees = 0;
                    break;

            }

            return GetAbsoluteOrientation(degrees);
        }





    }
}
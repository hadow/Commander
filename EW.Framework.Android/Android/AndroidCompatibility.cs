using System;
using System.Collections.Generic;
using Android.Views;
using Android.Content.Res;

namespace EW.Framework.Mobile
{
    /// <summary>
    /// 
    /// </summary>
    public static class AndroidCompatibility
    {
        public static Lazy<Orientation> NaturalOrientation { get; private set; }

        public static bool FlipLandscape { get; private set; }

        static AndroidCompatibility()
        {
            NaturalOrientation = new Lazy<Orientation>(GetDeviceNaturalOrientation);
        }

        /// <summary>
        /// 获取设备的朝向
        /// </summary>
        /// <returns></returns>
        private static Orientation GetDeviceNaturalOrientation()
        {
            var orientation = Game.Activity.Resources.Configuration.Orientation;
            SurfaceOrientation rotation = Game.Activity.WindowManager.DefaultDisplay.Rotation;

            if(((rotation == SurfaceOrientation.Rotation0 || rotation == SurfaceOrientation.Rotation180) && orientation == Orientation.Landscape) ||
                ((rotation == SurfaceOrientation.Rotation90 || rotation == SurfaceOrientation.Rotation270) && orientation == Orientation.Portrait))
            {
                return Orientation.Landscape;
            }
            else
            {
                return Orientation.Portrait;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="orientation"></param>
        /// <returns></returns>
        internal static DisplayOrientation GetAbsoluteOrientation(int orientation)
        {
            if (NaturalOrientation.Value == Orientation.Landscape)
                orientation += 270;

            int ort = ((orientation + 45) / 90 * 90) % 360;
            var disporientation = DisplayOrientation.Unknown;

            switch (ort)
            {
                case 0:
                    disporientation = DisplayOrientation.Portrait;
                    break;
                case 90:
                    disporientation = FlipLandscape ? DisplayOrientation.LandscapeLeft : DisplayOrientation.LandscapeRight;
                    break;
                case 180:
                    disporientation = DisplayOrientation.PortraitDown;
                    break;
                case 270:
                    disporientation = FlipLandscape ? DisplayOrientation.LandscapeRight : DisplayOrientation.LandscapeLeft;
                    break;
                default:
                    disporientation = DisplayOrientation.LandscapeLeft;
                    break;
            }

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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.Views;
using EW.Framework.Mobile;
namespace EW.Framework.Graphics
{
    public sealed class GraphicsAdapter:IDisposable
    {

        public enum DriverType
        {
            /// <summary>
            /// Hardware device been used for rendering,Maximum speed and performance.
            /// </summary>
            Hardware,

            /// <summary>
            /// Emulates the hardware device on CPU,Slowly,only for testing.
            /// </summary>
            Reference,

            FastSoftware
        }



        /// <summary>
        /// Used to request creation of a specific kind of driver.
        /// </summary>
        public static DriverType UseDriverType { get; set; }


        public static bool UseReferenceDevice
        {
            get { return UseDriverType == DriverType.Reference; }
            set
            {
                UseDriverType = value ? DriverType.Reference : DriverType.Hardware;
            }
        }


        private static ReadOnlyCollection<GraphicsAdapter> _adapters;

        public static ReadOnlyCollection<GraphicsAdapter> Adapters
        {
            get
            {
                if(_adapters == null)
                {
                    _adapters = new ReadOnlyCollection<GraphicsAdapter>(new[] { new GraphicsAdapter() });
                }

                return _adapters;
            }
        }

        public static GraphicsAdapter DefaultAdapter
        {
            get { return Adapters[0]; }
        }


        public bool IsProfileSupported(GraphicsProfile graphicsProfile)
        {
            if (UseReferenceDevice)
                return true;

            switch (graphicsProfile)
            {
                case GraphicsProfile.Reach:
                    return true;
                case GraphicsProfile.HiDef:
                    bool result = true;
                    return result;
                default:
                    throw new InvalidOperationException();
            }
        }

        public DisplayMode CurrentDisplayMode
        {
            get
            {
                View view = ((AndroidGameWindow)Game.Instance.Window).GameView;
                //return new DisplayMode(view.Width, view.Height, SurfaceFormat.Color);
                return new DisplayMode(view.Width, view.Height, SurfaceFormat.Bgra32);
                //return new DisplayMode(1280, 720, SurfaceFormat.Color);
            }
        }
        public void Dispose()
        {

        }


    }
}
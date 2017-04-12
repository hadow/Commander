using System;
using System.Collections.ObjectModel;
using Android.Views;
namespace EW.Xna.Platforms.Graphics
{
    /// <summary>
    /// 图形适配器
    /// </summary>
    public sealed class GraphicsAdapter:IDisposable
    {

        public enum DriverT
        {
            Hardware,//硬件加速
            Reference,//模拟硬件(仅供测试用)
            FastSoftware,//软件驱动(仅当设备不支持硬件加速)
        }

        string _description = string.Empty;
        public string Description { get { return _description; }set { _description = value; } }

        private static ReadOnlyCollection<GraphicsAdapter> _adapters;
        GraphicsAdapter() { }

        public void Dispose()
        {

        }

        public DisplayMode CurrentDisplayMode
        {
            get
            {
#if ANDROID
                View view = ((AndroidGameWindow)Game.Instance.Window).GameView;
                return new DisplayMode(view.Width, view.Height, SurfaceFormat.Color);
#endif
            }
        }

        public static GraphicsAdapter DefaultAdapter
        {
            get { return Adapters[0]; }
        }
        public static ReadOnlyCollection<GraphicsAdapter> Adapters
        {
            get
            {
                if(_adapters == null)
                {
#if ANDROID
                    _adapters = new ReadOnlyCollection<GraphicsAdapter>(new[] { new GraphicsAdapter() });

#endif
                }
                return _adapters;
            }
        }



        /// <summary>
        /// 图形配置是否支持
        /// </summary>
        /// <param name="graphicsProfile"></param>
        /// <returns></returns>
        public bool IsProfileSupported(GraphicsProfile graphicsProfile)
        {

            switch (graphicsProfile)
            {
                case GraphicsProfile.Reach:
                    return true;
                    break;
                case GraphicsProfile.HiDef:
                    bool result = true;
                    //TODO:check adapter capabilities....
                    return result;
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }




    }
}
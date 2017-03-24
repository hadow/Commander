

namespace EW.Mobile.Platforms.Graphics
{

    public enum GraphicsProfile
    {
        Reach,
        HiDef,
    }

    /// <summary>
    /// 图形设备信息
    /// </summary>
    public class GraphicsDeviceInformation
    {

        public GraphicsAdapter Adapter { get; set; }

        public GraphicsProfile GraphicsProfile { get; set; }

        public PresentationParameters PresentationParameters { get; set; }

    }
}
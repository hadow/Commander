using System;


namespace EW.OpenGLES.Graphics
{
    public class GraphicsDeviceInformation
    {
        public GraphicsAdapter Adapter { get; set; }


        /// <summary>
        /// The requested graphics device feature set.
        /// </summary>
        public GraphicsProfile GraphicsProfile { get; set; }

        /// <summary>
        /// This settings that define how graphics will be presented to the display.
        /// </summary>
        public PresentationParameters PresentationParameters { get; set; }
        
    }
}
using System;


namespace EW.Framework.Graphics
{
    public class PreparingDeviceSettingsEventArgs:EventArgs
    {
        public GraphicsDeviceInformation GraphicsDeviceInformation { get; private set; }

        public PreparingDeviceSettingsEventArgs(GraphicsDeviceInformation graphicsDeviceInformation)
        {
            GraphicsDeviceInformation = graphicsDeviceInformation;
        }



    }
}
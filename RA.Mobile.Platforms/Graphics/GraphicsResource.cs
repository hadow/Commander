using System;
using System.Collections.Generic;


namespace RA.Mobile.Platforms.Graphics
{
    public abstract class GraphicsResource:IDisposable
    {
        GraphicsDevice _graphicsDevice;

        public GraphicsDevice GraphicsDevice
        {
            get { return _graphicsDevice; }
            internal set
            {
                if (_graphicsDevice == value)
                    return;
                if (_graphicsDevice != null)
                {

                }
                _graphicsDevice = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        internal protected virtual void GraphicsDeviceResetting()
        {

        }

        public void Dispose()
        {

        }
    }
}
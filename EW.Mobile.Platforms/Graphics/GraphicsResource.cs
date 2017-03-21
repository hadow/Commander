using System;
using System.Collections.Generic;


namespace EW.Mobile.Platforms.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class GraphicsResource:IDisposable
    {
        bool disposed;
        GraphicsDevice _graphicsDevice;
        private WeakReference _selfReference;
        public event EventHandler<EventArgs> Disposing;


        internal GraphicsResource()
        {

        }

        ~GraphicsResource()
        {
            Dispose(false);
        }


        public GraphicsDevice GraphicsDevice
        {
            get { return _graphicsDevice; }
            internal set
            {
                if (_graphicsDevice == value)
                    return;
                if (_graphicsDevice != null)
                {
                    _graphicsDevice.RemoveResourceReference(_selfReference);
                    _selfReference = null;
                }
                _graphicsDevice = value;
                _selfReference = new WeakReference(this);
                _graphicsDevice.AddResourceReference(_selfReference);
            }
        }

        public bool IsDisposed { get { return disposed; } }


        /// <summary>
        /// 
        /// </summary>
        internal protected virtual void GraphicsDeviceResetting()
        {

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    //Release managed objects
                }


                if(disposing && Disposing != null)
                {
                    Disposing(this, EventArgs.Empty);
                }

                if (_graphicsDevice != null)
                    _graphicsDevice.RemoveResourceReference(_selfReference);

                _selfReference = null;
                _graphicsDevice = null;
                disposed = true;

            }
        }


        public string Name { get; set; }

        public Object Tag { get; set; }


        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? base.ToString() : Name;
        }
    }
}
using System;

namespace EW.OpenGLES.Graphics
{
    public abstract class GraphicsResource:IDisposable
    {



        bool disposed;

        GraphicsDevice graphicsDevice;

        private WeakReference _selfReference;

        ~GraphicsResource()
        {
            //Pass false so the managed objects are not released
            Dispose(false);
        }


        public GraphicsDevice GraphicsDevice
        {
            get { return graphicsDevice; }
            internal set
            {
                if (graphicsDevice == value)
                    return;

                if(graphicsDevice != null)
                {
                    graphicsDevice.RemoveResourceReference(_selfReference);
                    _selfReference = null;
                }


                graphicsDevice = value;

                _selfReference = new WeakReference(this);
                graphicsDevice.AddResourceReference(_selfReference);
            }
        }

        public void Dispose()
        {
            //Dispose of managed objects as well
            Dispose(true);
            //Since we have been manually disposed,do not call the finalizer on this object.
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {

                }

                if (graphicsDevice != null)
                {
                    graphicsDevice.RemoveResourceReference(_selfReference);
                }

                _selfReference = null;
                graphicsDevice = null;
                disposed = true;
            }
        }
    }
}
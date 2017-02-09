using System;

namespace RA.Mobile.Platforms.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    public partial class GraphicsDevice:IDisposable
    {
        private Viewport _viewport;

        public Viewport Viewport
        {
            get
            {
                return _viewport;
            }
            set
            {
                _viewport = value;
                PlatformSetViewport(ref value);
            }
        }

        public PresentationParameters PresentationParameters
        {
            get;private set;
        }


        /// <summary>
        /// ≥ı ºªØ
        /// </summary>
        internal void Initialize()
        {
            PlatformInitialize();


        }


        private void PlatformSetViewport(ref Viewport value)
        {

        }



        /// <summary>
        /// 
        /// </summary>
        public void Present()
        {

        }

        internal void OnDeviceResetting()
        {
            
        }

        public void Dispose()
        {

        }

        protected virtual void Dispose(bool disposing)
        {

        }
    }
}
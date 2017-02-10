using System;
using OpenTK.Graphics.ES20;
namespace RA.Mobile.Platforms.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    public partial class GraphicsDevice:IDisposable
    {

        private bool _vertexShaderDirty;
        private bool VertexShaderDirty
        {
            get { return _vertexShaderDirty; }
        }

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

        private int _currentRenderTargetCount;
        internal bool IsRenderTargetBound
        {
            get
            {
                return _currentRenderTargetCount > 0;
            }
        }

        
        /// <summary>
        /// 
        /// </summary>
        public void Present()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        public void Clear(Color color)
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
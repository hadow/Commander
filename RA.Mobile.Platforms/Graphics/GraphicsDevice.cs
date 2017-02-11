using System;
using System.Collections;
using System.Collections.Generic;

using OpenTK.Graphics.ES20;
namespace RA.Mobile.Platforms.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    public partial class GraphicsDevice:IDisposable
    {

        public event EventHandler<EventArgs> DeviceLost;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;

        private readonly object _resourceLock = new object();

        private readonly List<WeakReference> _resources = new List<WeakReference>();

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

        /// <summary>
        /// 
        /// </summary>
        internal void OnDeviceResetting()
        {
            if (DeviceResetting != null)
                DeviceResetting(this, EventArgs.Empty);
            lock (_resourceLock)
            {
                foreach(var resource in _resources)
                {
                    var target = resource.Target as GraphicsResource;
                    if (target != null)
                        target.GraphicsDeviceResetting();
                }
                _resources.RemoveAll(wr => !wr.IsAlive);
            }

        }

        internal void AddResourceReference(WeakReference resourceReference)
        {
            lock (_resourceLock)
                _resources.Add(resourceReference);
        }

        internal void RemoveResourceReference(WeakReference resourceReference)
        {
            lock (_resourceLock)
                _resources.Remove(resourceReference);
        }

        /// <summary>
        /// 
        /// </summary>
        internal void OnDeviceReset()
        {
            if (DeviceReset != null)
                DeviceReset(this, EventArgs.Empty);
        }

        public GraphicsAdapter Adapter
        {
            get;private set;
        }
        public DisplayMode DisplayMode
        {
            get { return Adapter.CurrentDisplayMode; }
        }

        public void Dispose()
        {

        }

        protected virtual void Dispose(bool disposing)
        {

        }
    }
}
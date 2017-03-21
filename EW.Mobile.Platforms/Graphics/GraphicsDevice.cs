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

        internal GraphicsDevice()
        {
            PresentationParameters = new PresentationParameters();
            PresentationParameters.DepthStencilFormat = DepthFormat.Depth24;
            Setup();
            GraphicsCapabilities = new GraphicsCapabilities();
            GraphicsCapabilities.Initialize(this);
            Initialize();
        }

        public GraphicsDevice(GraphicsAdapter adapter, GraphicsProfile graphicsProfile, PresentationParameters presentationParameters)
        {
            if (adapter == null)
                throw new ArgumentNullException("adapter");
            if (!adapter.IsProfileSupported(graphicsProfile))
                throw new NoSuitableGraphicsDeviceException(string.Format("Adapter '{0}' does not support the {1} profile.", adapter.Description, graphicsProfile));
            if (presentationParameters == null)
                throw new ArgumentNullException("presentationParameters");

            Adapter = adapter;
            PresentationParameters = presentationParameters;
            _graphicsProfile = graphicsProfile;
            Setup();
            GraphicsCapabilities = new GraphicsCapabilities();
            GraphicsCapabilities.Initialize(this);
            Initialize();
        }


        private void Setup()
        {

        }


        public event EventHandler<EventArgs> DeviceLost;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;

        private readonly object _resourceLock = new object();

        private readonly List<WeakReference> _resources = new List<WeakReference>();

        private readonly GraphicsProfile _graphicsProfile;
        public GraphicsProfile GraphicsProfile
        {
            get { return _graphicsProfile; }
        }



        internal GraphicsCapabilities GraphicsCapabilities { get; private set; }
        /// <summary>
        /// 顶点着色器
        /// </summary>
        private Shader _vertexShader;
        private bool _vertexShaderDirty;
        private bool VertexShaderDirty
        {
            get { return _vertexShaderDirty; }
        }

        private Shader _pixelShader;
        private bool _pixelShaderDirty;
        private bool PixelShaderDirty
        {
            get { return _pixelShaderDirty; }
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
        /// 初始化
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
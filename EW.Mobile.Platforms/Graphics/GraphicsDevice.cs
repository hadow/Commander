using System;
using System.Collections;
using System.Collections.Generic;

using OpenTK.Graphics.ES20;
namespace EW.Mobile.Platforms.Graphics
{

    public enum ClearOptions
    {
        /// <summary>
        /// Color buffer
        /// </summary>
        Target = 1,
        /// <summary>
        /// Depth buffer
        /// </summary>
        DepthBuffer = 2,
        /// <summary>
        /// Stencil buffer
        /// </summary>
        Stencil = 4
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class GraphicsDevice:IDisposable
    {
        /// <summary>
        /// 渲染信息(调试和诊断)
        /// </summary>
        internal GraphicsMetrics _graphicsMetrics;

        public GraphicsMetrics Metrics { get { return _graphicsMetrics; } set { _graphicsMetrics = value; } }

        private IndexBuffer _indexBuffer;

        private bool _indexBufferDirty;

        private VertexBufferBindings _vertexBuffers;
        private bool _vertexBuffersDirty;
        private int _maxVertexBufferSlots;

        private readonly ConstantBufferCollection _vertexConstantBuffers = new ConstantBufferCollection(ShaderStage.Vertex, 16);
        private readonly ConstantBufferCollection _pixelConstantBuffers = new ConstantBufferCollection(ShaderStage.Pixel, 16);


        private BlendState _blendStateAdditive;
        private BlendState _blendStateAlphaBlend;
        private BlendState _blendStateNonPremultiplied;
        private BlendState _blendStateOpaque;

        private Color _blendFactor = Color.White;
        private bool _blendFactorDirty;

        private int _currentRenderTargetCount;  //当前正待渲染目标数量

        internal bool IsRenderTargetBound
        {
            get { return _currentRenderTargetCount > 0; }
        }

        private Rectangle _scissorRectangle;//裁剪矩形范围
        private bool _scissorRectangleDirty;


        public Rectangle ScissorRectangle
        {
            get { return _scissorRectangle; }
            set
            {
                if (_scissorRectangle == value)
                    return;
                _scissorRectangle = value;
                _scissorRectangleDirty = true;
            }
        }

        private DepthStencilState _depthStencilState;

        public DepthStencilState DepthStencilState
        {
            get { return _depthStencilState; }
            set
            {
                if (_depthStencilState == value)
                    return;
                _depthStencilState = value;
            }
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

        internal Shader VertexShader
        {
            get { return _vertexShader; }
            set
            {
                if (_vertexShader == value)
                    return;
                _vertexShader = value;
                _vertexConstantBuffers.Clear();
                _vertexShaderDirty = true;
            }
        }

        /// <summary>
        /// 标识顶点shader是否已废弃
        /// </summary>
        private bool _vertexShaderDirty;
        private bool VertexShaderDirty
        {
            get { return _vertexShaderDirty; }
        }

        /// <summary>
        /// 片段着色器
        /// </summary>
        private Shader _pixelShader;

        internal Shader PixelShader
        {
            get { return _pixelShader; }
            set
            {
                if (_pixelShader == value)
                    return;
                _pixelShader = value;
                _pixelConstantBuffers.Clear();
                _pixelShaderDirty = true;
            }
        }

        /// <summary>
        /// 标识像素shader 是否已废弃
        /// </summary>
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

        internal GraphicsDevice()
        {
            PresentationParameters = new PresentationParameters();
            PresentationParameters.DepthStencilFormat = DepthFormat.Depth24;
            Setup();
            GraphicsCapabilities = new GraphicsCapabilities(this);
            GraphicsCapabilities.Initialize(this);
            Initialize();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        internal void Initialize()
        {
            PlatformInitialize();

            _vertexBuffers = new VertexBufferBindings(_maxVertexBufferSlots);
            _vertexBuffersDirty = true;

            _vertexShaderDirty = true;
            _pixelShaderDirty = true;
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
            GraphicsCapabilities = new GraphicsCapabilities(this);
            GraphicsCapabilities.Initialize(this);
            Initialize();
        }
        private void Setup()
        {

        }

       

        
        /// <summary>
        /// 呈现在屏幕上
        /// </summary>
        public void Present()
        {
            if (_currentRenderTargetCount != 0)
                throw new InvalidOperationException("Cannot call Present when a render target is active");

            _graphicsMetrics = new GraphicsMetrics();
            PlatformPresent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        public void Clear(Color color)
        {
            var options = ClearOptions.Target;
            options |= ClearOptions.DepthBuffer;
            options |= ClearOptions.Stencil;
            PlatformClear(options, color.ToVector4(), _viewport.MaxDepth, 0);

            unchecked
            {
                _graphicsMetrics._clearCount++;
            }
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

        public Color BlendFactor
        {
            get { return _blendFactor; }
            set
            {
                if (_blendFactor == value)
                    return;
                _blendFactor = value;
                _blendFactorDirty = true;
            }
        }

        public BlendState BlendState
        {
            get { return _blendState; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("BleneState Null");

                if (_blendState == value)
                    return;

                _blendState = value;

                var newBlendState = _blendState;
                if (ReferenceEquals(_blendState, BlendState.Additive))
                    newBlendState = _blendStateAdditive;
                else if (ReferenceEquals(_blendState, BlendState.AlphaBlend))
                    newBlendState = _blendStateAlphaBlend;
                else if (ReferenceEquals(_blendState, BlendState.NonPremultiplied))
                    newBlendState = _blendStateNonPremultiplied;
                else if (ReferenceEquals(_blendState, BlendState.Opaque))
                    newBlendState = _blendStateOpaque;

                newBlendState.BindToGraphicsDevice(this);
                _actualBlendState = newBlendState;
                BlendFactor = _actualBlendState.BlendFactor;
                _blendStateDirty = true;
            }
        }





        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primitiveT"></param>
        /// <param name="vertexData"></param>
        /// <param name="vertexOffset"></param>
        /// <param name="numVertices"></param>
        /// <param name="indexData"></param>
        /// <param name="indexOffset"></param>
        /// <param name="primitiveCount"></param>
        /// <param name="vertexDeclaration"></param>
        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveT,T[] vertexData,int vertexOffset,
            int numVertices,short[] indexData,int indexOffset,int primitiveCount,VertexDeclaration vertexDeclaration) where T:struct
        {
            if (vertexData == null || vertexData.Length == 0)
                throw new ArgumentNullException("vertexData");
            if (vertexOffset < 0 || vertexOffset >= vertexData.Length)
                throw new ArgumentNullException("vertexOffset");
            if (numVertices <= 0 || numVertices > vertexData.Length)
                throw new ArgumentNullException("numVertices");
            if (vertexOffset + numVertices > vertexData.Length)
                throw new ArgumentOutOfRangeException("numVertices & vertexOffset out of index");
            if (indexData == null || indexData.Length == 0)
                throw new ArgumentNullException("indexData");
            if (indexOffset < 0 || indexOffset >= indexData.Length)
                throw new ArgumentOutOfRangeException("indexOffset");
            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");
            if (indexOffset + GetElementCountArray(primitiveT, primitiveCount) > indexData.Length)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (vertexDeclaration == null)
                throw new ArgumentNullException("vertexDeclaration");


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="primitiveT"></param>
        /// <param name="primitiveCount"></param>
        /// <returns></returns>
        private static int GetElementCountArray(PrimitiveType primitiveT,int primitiveCount)
        {
            switch (primitiveT)
            {
                case PrimitiveType.LineList:
                    return primitiveCount * 2;
                case PrimitiveType.LineStrip:
                    return primitiveCount + 1;
                case PrimitiveType.TriangleList:
                    return primitiveCount * 3;
                case PrimitiveType.TriangleStrip:
                    return primitiveCount + 2;

            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="applyShaders"></param>
        internal void ApplyState(bool applyShaders)
        {
            PlatformBeginApplyState();

            PlatformApplyBlend();

            PlatformApplyState(applyShaders);

        }

        public void Dispose()
        {

        }

        protected virtual void Dispose(bool disposing)
        {

        }
    }
}
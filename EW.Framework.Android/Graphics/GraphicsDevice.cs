using System;
using System.Collections.Generic;
using System.Drawing;
namespace EW.Framework.Graphics
{
   
    public class GraphicsDevice:IGraphicsDevice
    {
        private Viewport _viewport;

        private bool _isDisposed;

        internal int glMajorVersion = 0;

        internal int glMinorVersion = 0;

        internal int _maxTextureSize = 0;

        internal int MaxVertexAttributes;

        //Resources may be added to and removed from the list from many threads.
        private readonly object _resourcesLock = new object();

        private readonly List<WeakReference> _resources = new List<WeakReference>();


        private readonly GraphicsProfile _graphicsProfile;


        internal GraphicsCapabilities GraphicsCapabilities { get; private set; }
        public GraphicsProfile GraphicsProfile
        {
            get { return _graphicsProfile; }
        }

        public GraphicsDevice(GraphicsAdapter adapter,GraphicsProfile graphicsProfile,PresentationParameters presentationParameters)
        {

            if (adapter == null)
                throw new ArgumentException("adapter");

            if (!adapter.IsProfileSupported(graphicsProfile))
                throw new Exception(string.Format("Adapter '{0}' does not support the {1} profile.", string.Empty, graphicsProfile));
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


        public GraphicsAdapter Adapter
        {
            get;
            private set;
        }


        internal void Initialize()
        {
            GL.EnableVertexAttribArray(Shader.VertexPosAttributeIndex);
            GraphicsExtensions.CheckGLError();
            GL.EnableVertexAttribArray(Shader.TexCoordAttributeIndex);
            GraphicsExtensions.CheckGLError();
            GL.EnableVertexAttribArray(Shader.TexMetadataAttributeIndex);
            GraphicsExtensions.CheckGLError();


        }

        public void Present()
        {

            Game.Instance.Platform.Present();


        }


        private void Setup()
        {

            //Initialize the main viewport
            _viewport = new Viewport(0, 0, DisplayMode.Width, DisplayMode.Height);
            _viewport.MaxDepth = 1.0f;


            int MaxTextureSlots = 16;

            GL.GetInteger(GetPName.MaxTextureImageUnits, out MaxTextureSlots);
            GraphicsExtensions.CheckGLError();

            GL.GetInteger(GetPName.MaxTextureSize, out _maxTextureSize);
            GraphicsExtensions.CheckGLError();

            GL.GetInteger(GetPName.MaxVertexAttribs, out MaxVertexAttributes);
            GraphicsExtensions.CheckGLError();

            try
            {
                string version = GL.GetString(StringName.Version);

                if (string.IsNullOrEmpty(version))
                    throw new Exception("Unable to retrieve OpenGL version");

                string[] versionSplit = version.Split(' ');
                if (versionSplit.Length > 2 && versionSplit[0].Equals("OpenGL") && versionSplit[1].Equals("ES"))
                {
                    glMajorVersion = Convert.ToInt32(versionSplit[2].Substring(0, 1));
                    glMinorVersion = Convert.ToInt32(versionSplit[2].Substring(2, 1));
                }
                else
                {
                    glMajorVersion = 1;
                    glMinorVersion = 1;
                }
            }
            catch (FormatException)
            {
                //if it fails we default to 1.1 context
                glMajorVersion = 1;
                glMinorVersion = 1;
            }
        }


        public DisplayMode DisplayMode
        {
            get
            {
                return Adapter.CurrentDisplayMode;
            }
        }


        public PresentationParameters PresentationParameters
        {
            get;private set;
        }


        public Viewport Viewport
        {
            get { return _viewport; }
            set
            {
                _viewport = value;
            }
        }

        internal void AddResourceReference(WeakReference resourceReference)
        {
            lock (_resourcesLock)
            {
                _resources.Add(resourceReference);
            }
        }


        internal void RemoveResourceReference(WeakReference resourceReference)
        {
            lock (_resourcesLock)
            {
                _resources.Remove(resourceReference);
            }
        }


        internal static Rectangle GetTitleSafeArea(int x,int y,int width,int height)
        {
            return new Rectangle(x, y, width, height);
        }

        public void SetBlendMode(BlendMode mode)
        {
            Threading.EnsureUIThread();
            GL.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
            GraphicsExtensions.CheckGLError();
            switch (mode)
            {
                case BlendMode.None:
                    GL.Disable(EnableCap.Blend);
                    break;
                case BlendMode.Alpha:
                    GL.Enable(EnableCap.Blend);
                    GraphicsExtensions.CheckGLError();
                    GL.BlendFunc((int)BlendingFactorSrc.One,(int)BlendingFactorSrc.OneMinusSrcAlpha);
                    break;
                case BlendMode.Additive:
                case BlendMode.Subtractive:
                    GL.Enable(EnableCap.Blend);
                    GraphicsExtensions.CheckGLError();
                    GL.BlendFunc((int)BlendingFactorSrc.One, (int)BlendingFactorDest.One);
                    if(mode == BlendMode.Subtractive)
                    {
                        GraphicsExtensions.CheckGLError();
                        GL.BlendEquation((int)BlendEquationMode.FuncReverseSubtract);
                    }
                    break;
                case BlendMode.Multiplicative:
                    GL.Enable(EnableCap.Blend);
                    GraphicsExtensions.CheckGLError();
                    GL.BlendFunc((int)BlendingFactorSrc.Zero, (int)BlendingFactorDest.SrcColor);
                    break;
                case BlendMode.DoubleMultiplicative:
                    GL.Enable(EnableCap.Blend);
                    GraphicsExtensions.CheckGLError();
                    GL.BlendFunc((int)BlendingFactorDest.DstColor, (int)BlendingFactorSrc.SrcColor);
                    break;

            }
        }



        public void Clear()
        {
            //Threading.EnsureUIThread();
            //GL.ClearColor(0, 0, 0, 1);
            //GraphicsExtensions.CheckGLError();
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //GraphicsExtensions.CheckGLError();
            Clear(Color.Black);
        }
        public void Clear(Color color)
        {
            Threading.EnsureUIThread();
            GL.ClearColor(color.R,color.G,color.B,color.A);
            GraphicsExtensions.CheckGLError();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GraphicsExtensions.CheckGLError();
        }

        static int ModelFromPrimitiveType(PrimitiveType pt)
        {
            switch (pt)
            {
                case PrimitiveType.PointList:
                    return (int)GLPrimitiveType.Points;
                case PrimitiveType.LineList:
                    return (int)GLPrimitiveType.Lines;
                case PrimitiveType.TriangleList:
                    return (int)GLPrimitiveType.Triangles;
            }

            throw new NotImplementedException();

        }



        public IShader CreateShader(string name)
        {
            return new Shader(name);
        }

        public void DrawPrimitives(PrimitiveType pt,int firstVertex,int numVertices)
        {
            GL.DrawArrays((GLPrimitiveType)ModelFromPrimitiveType(pt), firstVertex, numVertices);
            GraphicsExtensions.CheckGLError();
        }


        public void EnableDepthBuffer()
        {
            Threading.EnsureUIThread();
            GL.Clear(ClearBufferMask.DepthBufferBit);
            GraphicsExtensions.CheckGLError();
            GL.Enable(EnableCap.DepthTest);
            GraphicsExtensions.CheckGLError();
            GL.DepthFunc(DepthFunction.Lequal);
            GraphicsExtensions.CheckGLError();
        }
        

        public void DisableDepthBuffer()
        {
            Threading.EnsureUIThread();
            GL.Disable(EnableCap.DepthTest);
            GraphicsExtensions.CheckGLError();
        }

        public void ClearDepthBuffer()
        {
            Threading.EnsureUIThread();
            GL.Clear(ClearBufferMask.DepthBufferBit);
            GraphicsExtensions.CheckGLError();
        }


        public void EnableScissor(int left,int top,int width,int height)
        {
            Threading.EnsureUIThread();

            if (width < 0)
                width = 0;

            if (height < 0)
                height = 0;

            var bottom = PresentationParameters.BackBufferHeight - (top + height);


            GL.Scissor(left, bottom, width, height);
            GraphicsExtensions.CheckGLError();
            GL.Enable(EnableCap.ScissorTest);
            GraphicsExtensions.CheckGLError();
        }

        public void DisableScissor()
        {
            Threading.EnsureUIThread();
            GL.Disable(EnableCap.ScissorTest);
            GraphicsExtensions.CheckGLError();
        }

        public IVertexBuffer<Vertex> CreateVertexBuffer(int size)
        {
            Threading.EnsureUIThread();
            return new VertexBuffer<Vertex>(size);
        }

        public ITexture CreateTexture()
        {
            Threading.EnsureUIThread();
            var texture = new Texture();
            texture.GraphicsDevice = this;
            return texture;
        }


        public IFrameBuffer CreateFrameBuffer(Size s)
        {
            Threading.EnsureUIThread();
            return new FrameBuffer(s);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    //Dispose of all remaining graphics resources before disposing of the graphics device
                    lock (_resourcesLock)
                    {
                        foreach(var resource in _resources.ToArray())
                        {
                            var target = resource.Target as IDisposable;
                            if (target != null)
                                target.Dispose();
                        }
                        _resources.Clear();
                    }
                }

                _isDisposed = true;
            }
        }
    }
}
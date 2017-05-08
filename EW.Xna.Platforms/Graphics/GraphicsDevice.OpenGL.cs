using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Graphics.ES20;
using FramebufferAttachment = OpenTK.Graphics.ES20.All;
using RenderbufferStorage = OpenTK.Graphics.ES20.All;
using GLPrimitiveType= OpenTK.Graphics.ES20.BeginMode;
namespace EW.Xna.Platforms.Graphics
{
    /// <summary>
    /// Graphics Device For OpenGL ES
    /// </summary>
    public partial class GraphicsDevice
    {
        /// <summary>
        /// 
        /// </summary>
        private class RenderTargetBindingArrayComparer : IEqualityComparer<RenderTargetBinding[]>
        {
            public bool Equals(RenderTargetBinding[] first,RenderTargetBinding[] second)
            {
                if (object.ReferenceEquals(first, second))
                    return true;

                if (first == null || second == null)
                    return false;
                if (first.Length != second.Length)
                    return false;

                for(var i = 0; i < first.Length; i++)
                {
                    if (first[i].RenderTarget != second[i].RenderTarget || first[i].ArraySlic != second[i].ArraySlic)
                        return false;
                }
                return true;
            }

            public int GetHashCode(RenderTargetBinding[] array)
            {

                return 0;
            }
        }



        static List<Action> disposeActions = new List<Action>();
        static object disposeActionsLock = new object();

        internal FramebufferHelper framebufferHelper;

        internal int glMajorVersion = 0;
        internal int glMinorVersion = 0;
        internal int glFramebuffer = 0;

        /// <summary>
        /// //最多支持顶点属性
        /// </summary>
        internal int MaxVertexAttributes;

        /// <summary>
        /// 最大纹理尺寸
        /// </summary>
        internal int MaxTextureSize = 0;
        /// <summary>
        /// 已启用顶点属性
        /// </summary>
        internal static readonly List<int> _enabledVertexAttributes = new List<int>();

        internal List<string> _extensions = new List<string>();

        private readonly ShaderProgramCache _programCache = new ShaderProgramCache();
        private ShaderProgram _shaderProgram = null;

        
        private bool _blendStateDirty;

        /// <summary>
        /// Keep track of last applies state to avoid redundant OpenGL calls
        /// </summary>
        internal bool _lastBlendEnable = false;
        internal BlendState _lastBlendState = new BlendState();

        private Vector4 _lastClearColor = Vector4.Zero;
        private float _lastClearDepth = 1.0f;
        private float _lastClearStencil = 0;


        private DepthStencilState clearDepthStencilState = new DepthStencilState { StencilEnable = true };
        internal DepthStencilState _lastDepthStencilState = new DepthStencilState();
        internal RasterizerState _lastRasterizerState = new RasterizerState();
        

        private Dictionary<RenderTargetBinding[], int> glFramebuffers = new Dictionary<RenderTargetBinding[], int>(new RenderTargetBindingArrayComparer());
        private Dictionary<RenderTargetBinding[], int> glResolveFramebuffeers = new Dictionary<RenderTargetBinding[], int>(new RenderTargetBindingArrayComparer());
        /// <summary>
        /// 链接顶点属性（Enable Or Disable）
        /// </summary>
        /// <param name="attrs"></param>
        internal void SetVertexAttributeArray(bool[] attrs)
        {
            for(int i = 0; i < attrs.Length; i++)
            {
                if (attrs[i] && !_enabledVertexAttributes.Contains(i))
                {
                    _enabledVertexAttributes.Add(i);
                    GL.EnableVertexAttribArray(i);
                    GraphicsExtensions.CheckGLError();
                }
                else if(!attrs[i] && _enabledVertexAttributes.Contains(i))
                {
                    _enabledVertexAttributes.Remove(i);
                    GL.DisableVertexAttribArray(i);
                    GraphicsExtensions.CheckGLError();
                }

            }
            
        }


        /// <summary>
        /// 
        /// </summary>
        private void PlatformSetup()
        {
            MaxTextureSlots = 16;

            GL.GetInteger(GetPName.MaxTextureImageUnits, out MaxTextureSlots);
            GraphicsExtensions.CheckGLError();

            GL.GetInteger(GetPName.MaxVertexAttribs, out MaxVertexAttributes);
            GraphicsExtensions.CheckGLError();

            GL.GetInteger(GetPName.MaxTextureSize, out MaxTextureSize);
            GraphicsExtensions.CheckGLError();


            try
            {
                string version = GL.GetString(StringName.Version);
                string[] versionSplit = version.Split(' ');
                if(versionSplit.Length >2 && versionSplit[0].Equals("OpenGL") && versionSplit[1].Equals("ES"))
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
                glMajorVersion = 1;
                glMinorVersion = 1;
            }

            _extensions = GetGLExtensions();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        List<string> GetGLExtensions()
        {
            List<string> extensions = new List<string>();
            var extstring = GL.GetString(StringName.Extensions);
            GraphicsExtensions.CheckGLError();
            if (!string.IsNullOrEmpty(extstring))
                extensions.AddRange(extstring.Split(' '));
            return extensions;
        }


        /// <summary>
        /// 绘制基元
        /// </summary>
        /// <param name="primitiveT"></param>
        /// <param name="vertextStart"></param>
        /// <param name="vertexCount"></param>
        private void PlatformDrawPrimitives(PrimitiveType primitiveT,int vertexStart,int vertexCount)
        {
            ApplyState(true);

            var programHash = _vertexShader.HashKey | _pixelShader.HashKey;
            _vertexBuffers.Get(0).VertexBuffer.VertexDeclaration.Apply(_vertexShader, IntPtr.Zero, programHash);

            GL.DrawArrays(PrimitiveTypeGL(primitiveT), vertexStart, vertexCount);

            GraphicsExtensions.CheckGLError();
        }


        private static GLPrimitiveType PrimitiveTypeGL(PrimitiveType primitiveType)
        {
            switch(primitiveType)
            {
                case PrimitiveType.LineList:
                    return GLPrimitiveType.Lines;
                case PrimitiveType.LineStrip:
                    return GLPrimitiveType.LineStrip;
                case PrimitiveType.TriangleList:
                    return GLPrimitiveType.Triangles;
                case PrimitiveType.TriangleStrip:
                    return GLPrimitiveType.TriangleStrip;
            }

            throw new ArgumentException();
        }



        /// <summary>
        /// 图形平台初始化
        /// </summary>
        private void PlatformInitialize()
        {
            _viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);
            _enabledVertexAttributes.Clear();
            _programCache.Clear();
            _shaderProgram = null;
            //if (GraphicsCapabilities.SupportsFramebufferObjectARB)
            //{
            //    framebufferHelper = new FramebufferHelper(this);
            //}
            framebufferHelper = FramebufferHelper.Create(this);

            //Force resetting states
            this.PlatformApplyBlend(true);
            this.DepthStencilState.PlatformApplyState(this, true);
            this.RasterizerState.PlatformApplyState(this, true);

            _maxVertexBufferSlots = 1;
        }

        /// <summary>
        /// 设定OpenGL 渲染窗口维度
        /// </summary>
        /// <param name="value"></param>
        private void PlatformSetViewport(ref Viewport value)
        {
            if (IsRenderTargetBound)
                GL.Viewport(value.X, value.Y, value.Width, value.Height);
            else
                GL.Viewport(value.X, PresentationParameters.BackBufferHeight - value.Y - value.Height, value.Width, value.Height);
            GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.Viewport");

#if GLES
            GL.DepthRange(value.MinDepth, value.MaxDepth);
#endif
            GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.DepthRange");

            _vertexShaderDirty = true;

        }


        /// <summary>
        /// 
        /// </summary>
        public void PlatformPresent()
        {
            GraphicsExtensions.CheckGLError();
            lock (disposeActionsLock)
            {
                if (disposeActions.Count > 0)
                {
                    foreach (var action in disposeActions)
                        action();
                    disposeActions.Clear();

                }
            }
        }


        /// <summary>
        /// 清空
        /// </summary>
        /// <param name="options"></param>
        /// <param name="color"></param>
        /// <param name="depth"></param>
        /// <param name="stencil"></param>
        public void PlatformClear(ClearOptions options,Vector4 color,float depth,int stencil)
        {
            var prevScissorRect = ScissorRectangle;
            var prevDepthStencilState = DepthStencilState;
            var prevBlendState = BlendState;

            ScissorRectangle = _viewport.Bounds;

            DepthStencilState = this.clearDepthStencilState;
            BlendState = BlendState.Opaque;

            ApplyState(false);

            ClearBufferMask bufferMask = 0;

            //颜色缓冲
            if((options & ClearOptions.Target) == ClearOptions.Target)
            {
                if(color != _lastClearColor)
                {
                    GL.ClearColor(color.X, color.Y, color.Z, color.W);//状态设置
                    GraphicsExtensions.CheckGLError();
                    _lastClearColor = color;
                }
                bufferMask = bufferMask | ClearBufferMask.ColorBufferBit;

            }
            //模板缓冲
            if((options & ClearOptions.Stencil)== ClearOptions.Stencil)
            {
                if(stencil != _lastClearStencil)
                {
                    GL.ClearStencil(stencil);
                    GraphicsExtensions.CheckGLError();
                    _lastClearStencil = stencil;
                }
                bufferMask = bufferMask | ClearBufferMask.StencilBufferBit;
            }

            //深度缓冲
            if((options & ClearOptions.DepthBuffer) == ClearOptions.DepthBuffer)
            {
                if(depth != _lastClearDepth)
                {
                    GL.ClearDepth(depth);
                    GraphicsExtensions.CheckGLError();
                    _lastClearDepth = depth;
                }
                bufferMask = bufferMask | ClearBufferMask.DepthBufferBit;
            }



            GL.Clear(bufferMask);//状态应用
            GraphicsExtensions.CheckGLError();

            //
            ScissorRectangle = prevScissorRect;
            DepthStencilState = prevDepthStencilState;
            BlendState = prevBlendState;
        }

        
        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveT,T[] vertexData,int vertexOffset,int numVertices,short[] indexData,int indexOffset,int primitiveCount,VertexDeclaration vertexDeclaration)
        {
            ApplyState(true);

            //Unbind current VBOs
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            _vertexBuffersDirty = _indexBufferDirty = true;

            //固定对象的地址，这样可以防止垃圾回收器移动对象，从而破坏垃圾收集器的效率，使用Free方法尽快释放分配的句柄
            var vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            var ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);

            var vertexAddr = (IntPtr)(vbHandle.AddrOfPinnedObject().ToInt64() + vertexDeclaration.VertexStride * vertexOffset);
            vertexDeclaration.GraphicsDevice = this;

            //设置顶点声明指向VertexBuffer Data
            var programHash = _vertexShader.HashKey | _pixelShader.HashKey;
            vertexDeclaration.Apply(_vertexShader, vertexAddr, programHash);

            GL.DrawElements(PrimitiveTypeGL(primitiveT),
                GetElementCountArray(primitiveT, primitiveCount),
                DrawElementsType.UnsignedShort,
                (IntPtr)(ibHandle.AddrOfPinnedObject().ToInt64() + (indexOffset * sizeof(short))));

            GraphicsExtensions.CheckGLError();

            //Release Handle
            ibHandle.Free();
            vbHandle.Free();
        }


        internal void PlatformBeginApplyState()
        {
            Threading.EnsureUIThread();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="force"></param>
        private void PlatformApplyBlend(bool force = false)
        {
            _actualBlendState.PlatformApplyState(this, force);
            ApplyBlendFactor(force);
        }


        private void ApplyBlendFactor(bool force)
        {
            if(force || BlendFactor != _lastBlendState.BlendFactor)
            {
                GL.BlendColor(this.BlendFactor.R / 255.0f, 
                            this.BlendFactor.G / 255.0f,
                            this.BlendFactor.B / 255.0f,
                            this.BlendFactor.A / 255.0f);
                GraphicsExtensions.CheckGLError();
                _lastBlendState.BlendFactor = this.BlendFactor;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="applyShaders"></param>
        internal void PlatformApplyState(bool applyShaders)
        {
            if (_scissorRectangleDirty)
            {
                var scissorRect = _scissorRectangle;
                if (!IsRenderTargetBound)
                    scissorRect.Y = _viewport.Height - scissorRect.Y - scissorRect.Height;
                GL.Scissor(scissorRect.X, scissorRect.Y, scissorRect.Width, scissorRect.Height);
                GraphicsExtensions.CheckGLError();
                _scissorRectangleDirty = false;
            }

            if (!applyShaders)
                return;

            if (_indexBufferDirty)
            {
                if (_indexBuffer != null)
                {
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer.ibo);
                    GraphicsExtensions.CheckGLError();
                }
                _indexBufferDirty = false;
            }

            if (_vertexBuffersDirty)
            {
                if (_vertexBuffers.Count > 0)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffers.Get(0).VertexBuffer.vbo);
                    GraphicsExtensions.CheckGLError();
                }
                _vertexBuffersDirty = false;
            }
            if (_vertexShader == null)
                throw new InvalidOperationException("A Vertex Shader Must Be Set!");
            if (_pixelShader == null)
                throw new InvalidOperationException("A Pixel Shader Must Be Set!");
            if(_vertexShaderDirty || _pixelShaderDirty)
            {
                ActivateShaderProgram();
                if (_vertexShaderDirty)
                {
                    unchecked
                    {
                        _graphicsMetrics._vertexShaderCount++;
                    }
                }

                if (_pixelShaderDirty)
                {
                    unchecked
                    {
                        _graphicsMetrics._pixelShaderCount++;
                    }
                }
                _vertexShaderDirty = _pixelShaderDirty = false;
            }

            _vertexConstantBuffers.SetConstantBuffers(this, _shaderProgram);
            _pixelConstantBuffers.SetConstantBuffers(this, _shaderProgram);

            Textures.SetTextures(this);
            
        }


        /// <summary>
        /// 
        /// </summary>
        private unsafe void ActivateShaderProgram()
        {
            var shaderProgram = _programCache.GetProgram(VertexShader, PixelShader);
            if (shaderProgram.Program == -1)
                return;
            if(_shaderProgram != shaderProgram)
            {
                GL.UseProgram(shaderProgram.Program);
                GraphicsExtensions.CheckGLError();
                _shaderProgram = shaderProgram;
            }

            var posFixupLoc = shaderProgram.GetUnitformLocation("posFixup");
            if (posFixupLoc == -1)
                return;

        }

        /// <summary>
        /// 
        /// </summary>
        private void PlatformApplyDefaultRenderTarget()
        {
            this.framebufferHelper.BindFramebuffer(this.glFramebuffer);

            _rasterizerStateDirty = true;

            //Textures will need to be reound to render correctly in the new render target
            Textures.Dirty();
        }

        /// <summary>
        /// 创建一个渲染目标
        /// </summary>
        /// <param name="renderTarget"></param>ActivateShaderProgram
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mipMap"></param>
        /// <param name="preferredFormat"></param>
        /// <param name="preferredDepthFormat"></param>
        /// <param name="preferredMultiSampleCount"></param>
        /// <param name="usage"></param>
        internal void PlatformCreateRenderTarget(IRenderTarget renderTarget,int width,int height,bool mipMap,SurfaceFormat preferredFormat,DepthFormat preferredDepthFormat,
            int preferredMultiSampleCount,RenderTargetUsage usage)
        {
            var color = 0;
            var depth = 0;
            var stencil = 0;

            if(preferredDepthFormat != DepthFormat.None)
            {
                var depthInternalFormat = RenderbufferStorage.DepthComponent16;
                var stencilInternalFormat = (RenderbufferStorage)0;
                switch (preferredDepthFormat)
                {
                    case DepthFormat.Depth16:
                        depthInternalFormat = RenderbufferStorage.DepthComponent16;
                        break;

                }

                if(depthInternalFormat != 0)
                {
                    this.framebufferHelper.GenRenderbuffer(out depth);
                    this.framebufferHelper.BindRenderbuffer(depth);
                }
            }

            if (color != 0)
                renderTarget.GLColorBuffer = color;
            else
                renderTarget.GLColorBuffer = renderTarget.GLTexture;

            renderTarget.GLDepthBuffer = depth;
            renderTarget.GLStencilBuffer = stencil;
        }

        /// <summary>
        /// 
        /// </summary>
        private void PlatformResolveRenderTargets()
        {
            if (_currentRenderTargetCount == 0)
                return;

            var renderTargetBinding = this._currentRenderTargetBindings[0];
            var renderTarget = renderTargetBinding.RenderTarget as IRenderTarget;

            if(renderTarget.MultiSampleCount > 0 && this.framebufferHelper.SupportsBlitFramebuffer)
            {
                var glResolveFramebuffer = 0;
                if(!this.glResolveFramebuffeers.TryGetValue(this._currentRenderTargetBindings,out glResolveFramebuffer))
                {
                    this.framebufferHelper.GenFramebuffer(out glResolveFramebuffer);
                    this.framebufferHelper.BindFramebuffer(glResolveFramebuffer);

                    for(var i = 0; i < this._currentRenderTargetCount; i++)
                    {
                        this.framebufferHelper.FramebufferTexture2D((int)(FramebufferAttachment.ColorAttachment0 + i), (int)renderTarget.GetFramebufferTarget(renderTargetBinding), renderTarget.GLTexture);
                    }

                    this.glResolveFramebuffeers.Add((RenderTargetBinding[])this._currentRenderTargetBindings.Clone(), glResolveFramebuffer);
                }
                else
                {
                    this.framebufferHelper.BindFramebuffer(glResolveFramebuffer);
                }
            }

            for(var i = 0; i < _currentRenderTargetCount; i++)
            {
                renderTargetBinding = _currentRenderTargetBindings[i];
                renderTarget = renderTargetBinding.RenderTarget as IRenderTarget;

                if (renderTarget.LevelCount > 1)
                {
                    GL.BindTexture(renderTarget.GLTarget, renderTarget.GLTexture);
                    GraphicsExtensions.CheckGLError();

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IRenderTarget PlatformApplyRenderTargets()
        {
            var glFramebuffer = 0;
            if(!this.glFramebuffers.TryGetValue(this._currentRenderTargetBindings,out glFramebuffer))
            {

            }
            else
            {
                this.framebufferHelper.BindFramebuffer(glFramebuffer);
            }
            _rasterizerStateDirty = true;

            Textures.Dirty();

            return _currentRenderTargetBindings[0].RenderTarget as IRenderTarget;
        }

        private void PlatformDispose()
        {
            _programCache.Dispose();
        }


    }
}
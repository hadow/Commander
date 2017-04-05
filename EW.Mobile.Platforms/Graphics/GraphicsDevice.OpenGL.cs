using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Graphics.ES20;
using GLPrimitiveType= OpenTK.Graphics.ES20.BeginMode;
namespace EW.Mobile.Platforms.Graphics
{
    /// <summary>
    /// Graphics Device For OPEN GL
    /// </summary>
    public partial class GraphicsDevice
    {

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
        /// 已启用顶点属性
        /// </summary>
        internal static readonly List<int> _enabledVertexAttributes = new List<int>();


        private readonly ShaderProgramCache _programCache = new ShaderProgramCache();
        private ShaderProgram _shaderProgram = null;

        private BlendState _blendState;
        private BlendState _actualBlendState;
        private bool _blendStateDirty;

        internal bool _lastBlendEnable = false;
        internal BlendState _lastBlendState = new BlendState();

        private Vector4 _lastClearColor = Vector4.Zero;
        private DepthStencilState clearDepthStencilState = new DepthStencilState { StencilEnable = true };

        private RasterizerState _rasterizerState;
        private RasterizerState _actualRasterizerState;
        private bool _rasterizerStateDirty;

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
        /// 平台初始化
        /// </summary>
        private void PlatformInitialize()
        {
            _viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);
            _enabledVertexAttributes.Clear();
            _programCache.Clear();
            _shaderProgram = null;
            if (GraphicsCapabilities.SupportsFramebufferObjectARB)
            {
                framebufferHelper = new FramebufferHelper(this);
            }
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
        /// 
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

            GL.Clear(bufferMask);//状态应用
            GraphicsExtensions.CheckGLError();

            ScissorRectangle = prevScissorRect;
            DepthStencilState = prevDepthStencilState;
            BlendState = prevBlendState;
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
        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveT,T[] vertexData,int vertexOffset,int numVertices,short[] indexData,int indexOffset,int primitiveCount,VertexDeclaration vertexDeclaration)
        {
            ApplyState(true);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            _vertexBuffersDirty = _indexBufferDirty = true;

            var vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            var ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);

            var vertexAddr = (IntPtr)(vbHandle.AddrOfPinnedObject().ToInt64() + vertexDeclaration.VertexStride * vertexOffset);
            vertexDeclaration.GraphicsDevice = this;

            var programHash = _vertexShader.HashKey | _pixelShader.HashKey;
            vertexDeclaration.Apply(_vertexShader, vertexAddr, programHash);

            GL.DrawElements(PrimitiveTypeGL(primitiveT), GetElementCountArray(primitiveT, primitiveCount), DrawElementsType.UnsignedShort, (IntPtr)(ibHandle.AddrOfPinnedObject().ToInt64() + (indexOffset * sizeof(short))));

            GraphicsExtensions.CheckGLError();

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
                GL.BlendColor(this.BlendFactor.R / 255.0f, this.BlendFactor.G / 255.0f, this.BlendFactor.B / 255.0f, this.BlendFactor.A / 255.0f);
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

        }

        /// <summary>
        /// 
        /// </summary>
        private void PlatformApplyDefaultRenderTarget()
        {
            this.framebufferHelper.BindFramebuffer(this.glFramebuffer);

            _rasterizerStateDirty = true;

            Textures.Dirty();
        }




    }
}
using System;
using OpenTK.Graphics.ES20;
using GLStencilFunction = OpenTK.Graphics.ES20.StencilFunction;
namespace EW.Xna.Platforms.Graphics
{
    public partial class DepthStencilState
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="force"></param>
        internal void PlatformApplyState(GraphicsDevice device,bool force = false)
        {
            //Enable/Disable …Ó∂»ª∫≥Â
            if(force || this.DepthBufferEnable != device._lastDepthStencilState.DepthBufferEnable)
            {
                if (!DepthBufferEnable)
                {
                    GL.Disable(EnableCap.DepthTest);
                    GraphicsExtensions.CheckGLError();
                }
                else
                {
                    GL.Enable(EnableCap.DepthTest);
                    GraphicsExtensions.CheckGLError();
                }
                device._lastDepthStencilState.DepthBufferEnable = this.DepthBufferEnable;
            }

            if(force || this.DepthBufferFunction != device._lastDepthStencilState.DepthBufferFunction)
            {
                GL.DepthFunc(this.DepthBufferFunction.GetDepthFunction());
                GraphicsExtensions.CheckGLError();
                device._lastDepthStencilState.DepthBufferFunction = this.DepthBufferFunction;
            }

            if(force || this.DepthBufferWriteEnable != device._lastDepthStencilState.DepthBufferWriteEnable)
            {
                GL.DepthMask(DepthBufferWriteEnable);
                GraphicsExtensions.CheckGLError();
                device._lastDepthStencilState.DepthBufferWriteEnable = this.DepthBufferWriteEnable;
            }

            if(force || this.StencilEnable != device._lastDepthStencilState.StencilEnable)
            {
                if (!StencilEnable)
                {
                    GL.Disable(EnableCap.StencilTest);
                    GraphicsExtensions.CheckGLError();

                }
                else
                {
                    GL.Enable(EnableCap.StencilTest);
                    GraphicsExtensions.CheckGLError();
                }
                device._lastDepthStencilState.StencilEnable = this.StencilEnable;
            }

            if(force || this.StencilWriteMask != device._lastDepthStencilState.StencilWriteMask)
            {
                GL.StencilMask(this.StencilWriteMask);
                GraphicsExtensions.CheckGLError();
                device._lastDepthStencilState.StencilWriteMask = StencilWriteMask;
                    
            }

            if (this.TwoSidedStencilMode)
            {

            }
            else
            {
                if(force || this.TwoSidedStencilMode != device._lastDepthStencilState.TwoSidedStencilMode ||
                    this.StencilFunction != device._lastDepthStencilState.StencilFunction ||
                    this.ReferenceStencil != device._lastDepthStencilState.ReferenceStencil ||
                    this.StencilMask != device._lastDepthStencilState.StencilMask)
                {
                    GL.StencilFunc(GetStencilFunc(this.StencilFunction), ReferenceStencil, this.StencilMask);
                    GraphicsExtensions.CheckGLError();

                    device._lastDepthStencilState.StencilFunction = StencilFunction;
                    device._lastDepthStencilState.ReferenceStencil = ReferenceStencil;
                    device._lastDepthStencilState.StencilMask = StencilMask;

                }

                if(force || this.TwoSidedStencilMode != device._lastDepthStencilState.TwoSidedStencilMode ||
                    this.StencilFail != device._lastDepthStencilState.StencilFail || 
                    this.StencilDetphBufferFail != device._lastDepthStencilState.StencilDetphBufferFail ||
                    this.StencilPass != device._lastDepthStencilState.StencilPass)
                {
                    GL.StencilOp(GetStencilOp(this.StencilFail), GetStencilOp(this.StencilDetphBufferFail), GetStencilOp(this.StencilPass));
                    GraphicsExtensions.CheckGLError();

                    device._lastDepthStencilState.StencilFail = this.StencilFail;
                    device._lastDepthStencilState.StencilDetphBufferFail = this.StencilDetphBufferFail;
                    device._lastDepthStencilState.StencilPass = this.StencilPass;
                }
            }

            device._lastDepthStencilState.TwoSidedStencilMode = this.TwoSidedStencilMode;
        }


        private static GLStencilFunction GetStencilFunc(CompareFunction function)
        {
            switch (function)
            {
                case CompareFunction.Always:
                    return GLStencilFunction.Always;
                case CompareFunction.Equal:
                    return GLStencilFunction.Equal;
                case CompareFunction.Greater:
                    return GLStencilFunction.Greater;
                case CompareFunction.GreaterEqual:
                    return GLStencilFunction.Gequal;
                case CompareFunction.Less:
                    return GLStencilFunction.Less;
                case CompareFunction.LessEqual:
                    return GLStencilFunction.Lequal;
                case CompareFunction.Never:
                    return GLStencilFunction.Never;
                case CompareFunction.NotEqual:
                    return GLStencilFunction.Notequal;
                default:
                    return GLStencilFunction.Always;
            }
        }

        private static StencilOp GetStencilOp(StencilOperation operation)
        {
            switch (operation)
            {
                case StencilOperation.Keep:
                    return StencilOp.Keep;
                case StencilOperation.Decrement:
                    return StencilOp.DecrWrap;
                default:
                    return StencilOp.Keep;
            }
        }



    }
}
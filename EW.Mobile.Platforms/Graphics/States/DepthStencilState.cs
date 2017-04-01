

namespace EW.Mobile.Platforms.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    public partial class DepthStencilState:GraphicsResource
    {

        private bool _stencilEnable;

        public bool StencilEnable
        {
            get { return _stencilEnable; }
            set
            {
                _stencilEnable = value;
            }
        }

    }
}
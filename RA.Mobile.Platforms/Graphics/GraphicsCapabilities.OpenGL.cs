using System;


namespace RA.Mobile.Platforms.Graphics
{
    internal partial class GraphicsCapabilities
    {

        //GL_ARB_framebuffer_object
        internal bool SupportsFramebufferObjectARB { get; private set; }

        //GL_EXT_framebuffer_object
        internal bool SupportsFramebufferObjectEXT { get; private set; }
    }
}
using System;

namespace EW.Xna.Platforms.Graphics
{
    internal partial class GraphicsCapabilities
    {

        public GraphicsCapabilities(GraphicsDevice graphicsDevice)
        {

        }
        internal bool SupportsNonPowerOfTwo { get; private set; }
        internal bool SupportsDxt1 { get; private set; }

        internal bool SupportsPvrtc { get; private set; }

        internal bool SupportsEtc1 { get; private set; }

        internal bool SupportsTextrueArrays { get; private set; }

        internal bool SupportsVertexTextures { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        internal void Initialize(GraphicsDevice device)
        {

        }
    }
}
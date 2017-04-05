using System;


namespace EW.Mobile.Platforms.Graphics
{
    public sealed partial class TextureCollection
    {


        private readonly GraphicsDevice _graphicsDevice;

        private readonly Texture[] _textures;

        private readonly bool _applyToVertexStage;

        private int _dirty;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="maxTextures"></param>
        /// <param name="applyToVertexStage"></param>
        internal TextureCollection(GraphicsDevice graphicsDevice,int maxTextures,bool applyToVertexStage)
        {
            _graphicsDevice = graphicsDevice;
            _textures = new Texture[maxTextures];
            _applyToVertexStage = applyToVertexStage;
            _dirty = int.MaxValue;
            PlatformInit();
        }

        public Texture this[int index]
        {
            get { return _textures[index]; }
            set
            {
                if (_applyToVertexStage && !_graphicsDevice.GraphicsCapabilities.SupportsVertexTextures)
                    throw new NotSupportedException("Vertex textures are not supported on this device");
                if (_textures[index] == value)
                    return;

                _textures[index] = value;
                _dirty |= 1 << index;
            }
        }


        internal void SetTextures(GraphicsDevice device)
        {
            if (_applyToVertexStage && !device.GraphicsCapabilities.SupportsVertexTextures)
                return;
            PlatformSetTextures(device);
        }

        internal void Dirty()
        {
            _dirty = int.MaxValue;
        }
    }
}
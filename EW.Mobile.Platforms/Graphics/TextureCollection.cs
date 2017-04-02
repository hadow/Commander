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
    }
}
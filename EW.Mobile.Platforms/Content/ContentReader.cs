using System;
using System.IO;
using EW.Mobile.Platforms.Graphics;
namespace EW.Mobile.Platforms.Content
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ContentReader:BinaryReader
    {
        private Action<IDisposable> recordDisposableObject;
        private ContentManager contentManager;
        private ContentTypeReaderManager typeReaderManager;
        private GraphicsDevice graphicsDevice;
        internal GraphicsDevice GraphicsDevice { get { return graphicsDevice; } }
        private string assetName;
        private ContentTypeReader[] typeReaders;

        internal ContentTypeReader[] TypeReaders { get { return typeReaders; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="stream"></param>
        /// <param name="graphicsDevice"></param>
        /// <param name="assetName"></param>
        /// <param name="version"></param>
        /// <param name="recordDisposableObject"></param>
        internal ContentReader(ContentManager manager,Stream stream,GraphicsDevice graphicsDevice,string assetName,int version,Action<IDisposable> recordDisposableObject) : base(stream)
        {
            contentManager = manager;
            this.graphicsDevice = graphicsDevice;
            this.assetName = assetName;
            this.recordDisposableObject = recordDisposableObject;
        }


    }
}
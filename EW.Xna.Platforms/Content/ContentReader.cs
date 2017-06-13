using System;
using System.IO;
using EW.Xna.Platforms.Graphics;
namespace EW.Xna.Platforms.Content
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ContentReader:BinaryReader
    {
        private Action<IDisposable> recordDisposableObject;
        private ContentManager contentManager;
        public ContentManager ContentManager
        {
            get { return contentManager; }
        }
        private ContentTypeReaderManager typeReaderManager;
        private GraphicsDevice graphicsDevice;
        internal GraphicsDevice GraphicsDevice { get { return graphicsDevice; } }
        private string assetName;
        public string AssetName
        {
            get { return assetName; }
        }
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

        internal object ReadAsset<T>()
        {
            InitializeTypeReaders();

            //Read primary object
            object result = ReadObject<T>();

            return result;
        }

        public T ReadObject<T>()
        {
            return InnerReadObject(default(T));
        }

        private T InnerReadObject<T>(T existingInstance)
        {
            var typeReaderIndex = Read7BitEncodedInt();
            if (typeReaderIndex == 0)
                return existingInstance;
            if (typeReaderIndex > typeReaders.Length)
                throw new Exception("Incorrect type reader index found!");

            var typeReader = typeReaders[typeReaderIndex - 1];
            var result = (T)typeReader.Read(this, existingInstance);
            RecordDisposable(result);
            return result;
        }

        /// <summary>
        /// 记录释放资源对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        private void RecordDisposable<T>(T result)
        {
            var disposable = result as IDisposable;
            if (disposable == null)
                return;

            if (recordDisposableObject != null)
                recordDisposableObject(disposable);
            else
                contentManager.RecordDisposable(disposable);
        }

        /// <summary>
        /// 初始化类型读取器
        /// </summary>
        internal void InitializeTypeReaders()
        {
            typeReaderManager = new ContentTypeReaderManager();
            typeReaders = typeReaderManager.LoadAssetReaders(this);
        }


        internal new int Read7BitEncodedInt()
        {
            return base.Read7BitEncodedInt();
        }

    }
}
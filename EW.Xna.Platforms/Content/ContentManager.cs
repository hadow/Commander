using System;
using System.IO;
using System.Collections.Generic;
using EW.Xna.Platforms.Graphics;
namespace EW.Xna.Platforms.Content
{
    /// <summary>
    /// 内容管理
    /// </summary>
    public partial class ContentManager:IDisposable
    {

        private bool disposed;

        private string _rootDirectory = string.Empty;
        public string RootDirectory { get { return _rootDirectory; } set { _rootDirectory = value; } }
        private static List<WeakReference> ContentManagers = new List<WeakReference>();

        private IServiceProvider serviceProvider;

        private IGraphicsDeviceService graphicsDeviceService;

        private Dictionary<string, object> loadedAssets = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        private List<IDisposable> disposableAssets = new List<IDisposable>();

        private static object ContentManagerLock = new object();

        private static readonly List<char> targetPlatformIdentifiers = new List<char>()
        {

            'i',//IOS
            'a',//Android

        };



        public ContentManager(IServiceProvider serviceProvider,string rootDirectory)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");
            if (string.IsNullOrEmpty(rootDirectory))
                throw new ArgumentNullException("rootDirectory");

            this.RootDirectory = rootDirectory;
            this.serviceProvider = serviceProvider;

            AddContentManager(this);
        }

        private static void AddContentManager(ContentManager contentManager)
        {
            bool contains = false;
            for(int i = ContentManagers.Count - 1; i >= 0; i--)
            {
                var contentRef = ContentManagers[i];
                if (ReferenceEquals(contentRef.Target, contentManager))
                    contains = true;
                if (!contentRef.IsAlive)
                    ContentManagers.RemoveAt(i);
            }

            if (!contains)
                ContentManagers.Add(new WeakReference(contentManager));
        }

        private static void RemoveContentManager(ContentManager contentManager)
        {
            lock (ContentManagerLock)
            {
                for(int i = ContentManagers.Count - 1; i >= 0; --i)
                {
                    var contentRef = ContentManagers[i];
                    if (!contentRef.IsAlive || ReferenceEquals(contentRef.Target, contentManager))
                        ContentManagers.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public virtual T Load<T>(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new ArgumentNullException("assetName");
            }
            if (disposed)
                throw new ObjectDisposedException("ContentManager");

            T result = default(T);

            var key = assetName.Replace('\\', '/');
            object asset = null;
            if(loadedAssets.TryGetValue(key,out asset))
            {
                if (asset is T)
                    return (T)asset;
            }

            result = ReadAsset<T>(assetName, null);

            loadedAssets[key] = result;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <param name="recordDisposableObject"></param>
        /// <returns></returns>
        protected T ReadAsset<T>(string assetName,Action<IDisposable> recordDisposableObject)
        {
            string originalAssetName = assetName;
            object result = null;
            if(this.graphicsDeviceService == null)
            {
                this.graphicsDeviceService = serviceProvider.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
                if (this.graphicsDeviceService == null)
                    throw new InvalidOperationException("No Graphics Device Service");
            }

            // 尝试当作一个XNB文件去加载
            var stream = OpenStream(assetName);
            using(var xnbReader = new BinaryReader(stream))
            {
                using (var reader = GetContentReaderFromXnb(assetName, stream, xnbReader, recordDisposableObject))
                {
                    result = reader.ReadAsset<T>();
                    if (result is GraphicsResource)
                        ((GraphicsResource)result).Name = originalAssetName;
                }
            }

            if (result == null)
                throw new Exception("Could not load " + originalAssetName + " asset!");

            return (T)result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="originalAssetName"></param>
        /// <param name="stream"></param>
        /// <param name="xnbReader"></param>
        /// <param name="recordDisposableObject"></param>
        /// <returns></returns>
        private ContentReader GetContentReaderFromXnb(string originalAssetName,Stream stream,BinaryReader xnbReader,Action<IDisposable> recordDisposableObject)
        {
            byte x = xnbReader.ReadByte();
            byte n = xnbReader.ReadByte();
            byte b = xnbReader.ReadByte();
            byte platform = xnbReader.ReadByte();

            if(x != 'X' || n !='N' || b!='B' || !targetPlatformIdentifiers.Contains((char)platform))
            {
                throw new Exception("Asset does not appear to be a valid XNB file,Did you process your content for Windows?");
            }

            byte version = xnbReader.ReadByte();
            byte flags = xnbReader.ReadByte();

            Stream decompressedStream = null;

            var reader = new ContentReader(this, decompressedStream, this.graphicsDeviceService.GraphicsDevice, originalAssetName, version, recordDisposableObject);
            return reader;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected virtual Stream OpenStream(string assetName)
        {
            Stream stream;
            try
            {
                var assetPath = Path.Combine(RootDirectory, assetName) + ".xnb";

                stream = TitleContainer.OpenStream(assetPath);
#if ANDROID
                MemoryStream memStream = new MemoryStream();
                stream.CopyTo(memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                stream.Close();
                stream = memStream;
#endif
            }
            catch(FileNotFoundException fileNotFound)
            {

                throw new FileNotFoundException("The content file was not found.", fileNotFound);
            }
            catch(DirectoryNotFoundException directoryNotFound)
            {
                throw new DirectoryNotFoundException("The directory was not found.", directoryNotFound);
            }
            catch(Exception exception)
            {
                throw new Exception("Opening stream error.", exception);
            }
            return stream;
        }

        /// <summary>
        /// 重载图形内容
        /// </summary>
        internal static void ReloadGraphicsContent()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposable"></param>
        internal void RecordDisposable(IDisposable disposable)
        {
            System.Diagnostics.Debug.Assert(disposable != null,"The disposable is null");
            if (!disposableAssets.Contains(disposable))
                disposableAssets.Add(disposable);
        }


        public virtual void Unload()
        {
            foreach(var disposable in disposableAssets)
            {
                if (disposable != null)
                    disposable.Dispose();
            }

            disposableAssets.Clear();
            loadedAssets.Clear();
        }



        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
            RemoveContentManager(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    Unload();
                disposed = true;
            }
        }



    }
}
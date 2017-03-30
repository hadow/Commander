using System;
using System.IO;
using System.Collections.Generic;
using EW.Mobile.Platforms.Graphics;
namespace EW.Mobile.Platforms.Content
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
                using(var reader = )
            }
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

        private 

        /// <summary>
        /// 重载图形内容
        /// </summary>
        internal static void ReloadGraphicsContent()
        {

        }

        public void Dispose() { }



    }
}
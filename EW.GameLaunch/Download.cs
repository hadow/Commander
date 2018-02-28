using System;
using System.ComponentModel;
using System.Net;
namespace EW
{
    public class Download
    {
        readonly object syncObject = new object();

        WebClient wc;
        public Download(string url,string path,Action<DownloadProgressChangedEventArgs> onProgress,Action<AsyncCompletedEventArgs> onComplete)
        {
            lock(syncObject)
            {
                wc = new WebClient { Proxy = null };
                wc.DownloadProgressChanged += (_, a) => onProgress(a);
                wc.DownloadFileCompleted += (_, a) => { DisposeWebClient();onComplete(a); };
                wc.DownloadDataAsync(new Uri(url));
            }
        }

        public Download(string url, Action<DownloadProgressChangedEventArgs> onProgress, Action<DownloadDataCompletedEventArgs> onComplete)
        {
            lock (syncObject)
            {
                wc = new WebClient { Proxy = null };
                wc.DownloadProgressChanged += (_, a) => onProgress(a);
                wc.DownloadDataCompleted += (_, a) => { DisposeWebClient(); onComplete(a); };
                wc.DownloadDataAsync(new Uri(url));
            }
        }

        void DisposeWebClient()
        {
            lock (syncObject)
            {
                wc.Dispose();
                wc = null;
            }
        }


    }
}
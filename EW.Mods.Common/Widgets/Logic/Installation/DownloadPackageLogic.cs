using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ComponentModel;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
namespace EW.Mods.Common.Widgets.Logic
{
    public class DownloadPackageLogic
    {

        readonly ModContent.ModDownload download;
        readonly Action onSuccess;

        string downloadHost;

        [ObjectCreator.UseCtor]
        public DownloadPackageLogic(ModContent.ModDownload download,Action onSuccess)
        {
            this.download = download;
            this.onSuccess = onSuccess;
        }


        void ShowDownloadDialog()
        {
            var file = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            Action deleteTempFile = () => {

                File.Delete(file);

            };
            //下载进度
            Action<DownloadProgressChangedEventArgs> onDownloadProgress = i =>
            {

            };

            //提取资源进度
            Action<string> onExtractProgress = s => { };

            Action<string> onError = s => { };

            //下载完成
            Action<AsyncCompletedEventArgs> onDownloadComplete = i =>
            {
                if (i.Cancelled)
                {
                    deleteTempFile();
                    return;
                }
                if (i.Error != null)
                {
                    deleteTempFile();
                    onError("");
                    return;
                }

                var extracted = new List<string>();//已经提取过的文件列表
                try
                {
                    using (var stream = File.OpenRead(file))
                    using(var z = new ZipFile(stream))
                    {
                        foreach(var kv in download.Extreact)
                        {
                            var entry = z.GetEntry(kv.Value);
                            if (entry == null || !entry.IsFile)
                                continue;

                            var targetPath = Platform.ResolvePath(kv.Key);
                            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                            extracted.Add(targetPath);

                            using (var zz = z.GetInputStream(entry))
                            using (var f = File.Create(targetPath))
                                zz.CopyTo(f);

                        }
                        z.Close();
                    }    
                }
                catch(Exception)
                {
                    foreach(var f in extracted)
                    {
                        File.Delete(f);
                    }
                    onError("Invalid archive");
                }
                finally
                {
                    deleteTempFile();
                }

            };

            //下载链接
            Action<string> downloadUrl = url =>
            {
                downloadHost = new Uri(url).Host;

                var dl = new Download(url, file, onDownloadProgress, onDownloadComplete);
                

            };


            if (download.MirrorList != null)
            {

            }
            else
                downloadUrl(download.URL);

        }


    }
}
using System.IO;
using System.Collections.Generic;
using Android.App;
using System.Linq;
namespace EW.FileSystem
{
    /// <summary>
    /// ÎÄ¼þ¼Ð
    /// </summary>
    public sealed class Folder:IReadWritePackage
    {
        readonly string path;

        public string Name { get { return path; } }
        private string[] files;
        public Folder(string path)
        {
            this.path = path;
            //if (!Directory.Exists(path))
            //    Directory.CreateDirectory(path);
        }
        
        public IEnumerable<string> Contents
        {
            get
            {
                //foreach(var filename in Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly))
                string[] lists = Application.Context.Assets.List(path);
                string[] files = lists.Where(x => x.IndexOf('.') != -1).ToArray();
                string[] directories = lists.Where(x => x.IndexOf('.') == -1).ToArray();
                if (this.files == null)
                    this.files = new string[files.Length];
                this.files = files;
                foreach (var filename in files)
                {
                    //yield return Path.GetFileName(filename);
                    yield return filename;
                }

                foreach (var directorname in directories)
                    //yield return Path.GetFileName(filename);
                    yield return directorname;
            }
        }


        public IReadOnlyPackage OpenPackage(string filename,FileSystem context)
        {
            var resolvedPath = Platform.ResolvePath(Path.Combine(Name, filename));
            //if (Directory.Exists(resolvedPath))
            //    return new Folder(resolvedPath);
            if (!filename.Contains("|") && string.IsNullOrEmpty(Path.GetExtension(resolvedPath))) //&& Directory.Exists(resolvedPath))
                return new Folder(resolvedPath);

            //Zip files loaded from Folders can be read-write
            IReadWritePackage readWritePackage;
            if (ZipFileLoader.TryParseReadWritePackage(resolvedPath, out readWritePackage))
                return readWritePackage;

            //Other package types can be loaded normally
            IReadOnlyPackage package;
            var s = GetStream(filename);
            if (s == null)
                return null;

            if (context.TryParsePackage(s, filename, out package))
                return package;
            s.Dispose();
            return null;
        }

        public void Update(string filename,byte[] content)
        {

        }

        public Stream GetStream(string filename)
        {
            try
            {
                var filePath = Path.Combine(path, filename);
                //return File.OpenRead(Path.Combine(path, filename));
                var stream = Android.App.Application.Context.Assets.Open(filePath);
#if ANDROID
                MemoryStream memStream = new MemoryStream();
                stream.CopyTo(memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                stream.Close();
                stream = memStream;
#endif
                return memStream;
            }

            catch { return null; }
        }

        public void Delete(string filename)
        {

        }

        public bool Contains(string filename)
        {
#if ANDROID
            return files != null && files.Contains(filename);
#elif WINDOWS
            var combined = Path.Combine(path, filename);
            return combined.StartsWith(path) && File.Exists(combined);
#endif
        }

        public void Dispose()
        {

        }
        
    }
}
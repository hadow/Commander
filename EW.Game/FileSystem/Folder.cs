using System;
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
                string[] directories = lists.SkipWhile(x => x.IndexOf('.') != -1).ToArray();
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

        public void Update(string filename,byte[] content)
        {

        }

        public Stream GetStream(string filename)
        {
            try
            {
                //return File.OpenRead(Path.Combine(path, filename));
                return Android.App.Application.Context.Assets.Open(Path.Combine(path, filename));
            }
            catch { return null; }
        }

        public void Delete(string filename)
        {

        }

        public bool Contains(string filename)
        {
            var combined = Path.Combine(path, filename);
            return combined.StartsWith(path) && File.Exists(combined);
        }

        public void Dispose()
        {

        }
        
    }
}
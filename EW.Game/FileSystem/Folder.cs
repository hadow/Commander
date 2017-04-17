using System;
using System.IO;
using System.Collections.Generic;

namespace EW.FileSystem
{
    public sealed class Folder:IReadWritePackage
    {
        readonly string path;

        public string Name { get { return path; } }
        public Folder(string path)
        {
            this.path = path;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public IEnumerable<string> Contents
        {
            get
            {
                foreach(var filename in Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly))
                {
                    yield return Path.GetFileName(filename);
                }
                foreach (var filename in Directory.GetDirectories(path))
                    yield return Path.GetFileName(filename);
            }
        }

        public void Update(string filename,byte[] content)
        {

        }

        public Stream GetStream(string filename)
        {
            try
            {
                return File.OpenRead(Path.Combine(path, filename));
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
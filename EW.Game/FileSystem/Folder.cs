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
    }
}
using System;
using System.IO;
using System.Collections.Generic;
using EW.Primitives;

namespace EW.FileSystem
{

    /// <summary>
    /// 
    /// </summary>
    public sealed class ZipFolder:IReadOnlyPackage
    {
        public ZipFile Parent { get; private set; }
        public string Name { get; private set; }

        readonly string path;
        public ZipFolder(FileSystem context,ZipFile parent,string path,string filename)
        {

        }

        public Stream GetStream(string filename)
        {
            return Parent.GetStream(path + '/' + filename);
        }

        public IEnumerable<string> Contents
        {
            get { return null; }
        }


        public bool Contains(string filename)
        {
            return Parent.Contains(path + '/' + filename);
        }

        public void Dispose() { }

    }
}
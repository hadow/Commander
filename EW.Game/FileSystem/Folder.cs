using System;


namespace EW.FileSystem
{
    public sealed class Folder:IReadWritePackage
    {
        readonly string path;
        public Folder(string path)
        {
            this.path = path;
        }
    }
}
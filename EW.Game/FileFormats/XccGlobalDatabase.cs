using System;
using System.Collections.Generic;
using System.IO;
namespace EW.FileFormats
{
    public class XccGlobalDatabase:IDisposable
    {

        public readonly string[] Entries;
        readonly Stream s;

        public XccGlobalDatabase(Stream stream)
        {

        }

        public void Dispose()
        {
            s.Dispose();
        }

    }
}
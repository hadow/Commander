using System;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace EW.Primitives
{
    public static class ZipFileHelper
    {
        public static ZipFile Create(Stream stream)
        {
            ZipConstants.DefaultCodePage = Encoding.UTF8.CodePage;
            return new ZipFile(stream);
        }


        public static ZipFile Create(FileStream stream)
        {
            ZipConstants.DefaultCodePage = Encoding.UTF8.CodePage;
            return new ZipFile(stream);
        }

    }
}
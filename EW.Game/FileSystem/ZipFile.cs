using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using SZipFile = ICSharpCode.SharpZipLib.Zip.ZipFile;
namespace EW.FileSystem
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ZipFile:IReadWritePackage
    {
        public IReadWritePackage Parent { get; private set; }
        public string Name { get; private set; }

        readonly Stream pkgStream;

        readonly SZipFile pkg;

        static ZipFile()
        {
            ZipConstants.DefaultCodePage = Encoding.UTF8.CodePage;
        }
        public ZipFile(Stream stream,string name,IReadOnlyPackage parent = null)
        {
            pkgStream = new MemoryStream();
            stream.CopyTo(pkgStream);
            pkgStream.Position = 0;

            Name = name;
            Parent = parent as IReadWritePackage;
            pkg = new SZipFile(pkgStream);
        }

        public ZipFile(IReadOnlyFileSystem context,string filename)
        {
            string name;
            IReadOnlyPackage p;
            if (!context.TryGetPackageContaining(filename, out p, out name))
                throw new FileNotFoundException("Unable to find parent package for {0}".F(filename));

            Name = name;
            Parent = p as IReadWritePackage;

            pkgStream = new MemoryStream();

            using (var sourceStream = p.GetStream(name))
                sourceStream.CopyTo(pkgStream);
            pkgStream.Position = 0;
            pkg = new SZipFile(pkgStream);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public Stream GetStream(string filename)
        {
            var entry = pkg.GetEntry(filename);
            if (entry == null)
                return null;

            using (var z = pkg.GetInputStream(entry))
            {
                var ms = new MemoryStream();
                z.CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
        }

        public IEnumerable<string> Contents
        {
            get
            {
                foreach (ZipEntry entry in pkg)
                    yield return entry.Name;

            }
        }

        public bool Contains(string filename)
        {
            return false;
        }

        public void Update(string filename,byte[] contents)
        {

        }

        public void Delete(string filename)
        {

        }


        public void Dispose()
        {

        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
namespace EW.FileSystem
{
    /// <summary>
    /// Attempt to parse a stream as this tyupe of package.
    /// If successful,the loader is expected to take ownership of 's' and dispose it once done.
    /// If unsuccessful,the loader is expected to return the stream position to where it started.
    /// </summary>
    public interface IPackageLoader
    {
        bool TryParsePackage(Stream s, string filename, FileSystem context, out IReadOnlyPackage package);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IReadOnlyPackage : IDisposable
    {
        string Name { get; }

        IEnumerable<string> Contents { get; }

        Stream GetStream(string filename);

        bool Contains(string filename);

        IReadOnlyPackage OpenPackage(string filename, FileSystem context);
    }

    public interface IReadWritePackage : IReadOnlyPackage
    {
        void Update(string filename, byte[] contents);
        void Delete(string filename);
    }
}
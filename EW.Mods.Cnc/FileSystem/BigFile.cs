using System;
using System.IO;
using System.Collections.Generic;
using EW.FileSystem;
using FS = EW.FileSystem.FileSystem;
using EW.Framework;
namespace EW.Mods.Cnc.FileSystem
{
    public class BigLoader : IPackageLoader
    {
        sealed class BigFile : IReadOnlyPackage
        {
            public string Name { get; private set; }
            public IEnumerable<string> Contents { get { return index.Keys; } }

            readonly Dictionary<string, Entry> index = new Dictionary<string, Entry>();
            readonly Stream s;

            public BigFile(Stream s, string filename)
            {
                Name = filename;
                this.s = s;

                try
                {
                    /* var signature = */
                    s.ReadASCII(4);

                    // Total archive size.
                    s.ReadUInt32();

                    var entryCount = s.ReadUInt32();
                    if (BitConverter.IsLittleEndian)
                        entryCount = Int2.Swap(entryCount);

                    // First entry offset? This is apparently bogus for EA's .big files
                    // and we don't have to try seeking there since the entries typically start next in EA's .big files.
                    s.ReadUInt32();

                    for (var i = 0; i < entryCount; i++)
                    {
                        var entry = new Entry(s);
                        index.Add(entry.Path, entry);
                    }
                }
                catch
                {
                    Dispose();
                    throw;
                }
            }

            class Entry
            {
                readonly Stream s;
                readonly uint offset;
                readonly uint size;
                public readonly string Path;

                public Entry(Stream s)
                {
                    this.s = s;

                    offset = s.ReadUInt32();
                    size = s.ReadUInt32();
                    if (BitConverter.IsLittleEndian)
                    {
                        offset = Int2.Swap(offset);
                        size = Int2.Swap(size);
                    }

                    Path = s.ReadASCIIZ();
                }

                public Stream GetData()
                {
                    s.Position = offset;
                    return new MemoryStream(s.ReadBytes((int)size));
                }
            }

            public Stream GetStream(string filename)
            {
                return index[filename].GetData();
            }

            public bool Contains(string filename)
            {
                return index.ContainsKey(filename);
            }

            public IReadOnlyPackage OpenPackage(string filename, FS context)
            {
                // Not implemented
                return null;
            }

            public void Dispose()
            {
                s.Dispose();
            }
        }

        bool IPackageLoader.TryParsePackage(Stream s, string filename, FS context, out IReadOnlyPackage package)
        {
            // Take a peek at the file signature
            var signature = s.ReadASCII(4);
            s.Position -= 4;

            if (signature != "BIGF")
            {
                package = null;
                return false;
            }

            package = new BigFile(s, filename);
            return true;
        }
    }
}
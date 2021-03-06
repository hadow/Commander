﻿using System;
using System.Collections.Generic;
using System.IO;
using EW.FileSystem;
using FS = EW.FileSystem.FileSystem;

namespace EW.Mods.Cnc.FileSystem
{
    public class PakFileLoader : IPackageLoader
    {
        struct Entry
        {
            public uint Offset;
            public uint Length;
            public string Filename;
        }

        sealed class PakFile : IReadOnlyPackage
        {
            public string Name { get; private set; }
            public IEnumerable<string> Contents { get { return index.Keys; } }

            readonly Dictionary<string, Entry> index = new Dictionary<string, Entry>();
            readonly Stream stream;

            public PakFile(Stream stream, string filename)
            {
                Name = filename;
                this.stream = stream;

                try
                {
                    var offset = stream.ReadUInt32();
                    while (offset != 0)
                    {
                        var file = stream.ReadASCIIZ();
                        var next = stream.ReadUInt32();
                        var length = (next == 0 ? (uint)stream.Length : next) - offset;

                        // Ignore duplicate files
                        if (index.ContainsKey(file))
                            continue;

                        index.Add(file, new Entry { Offset = offset, Length = length, Filename = file });
                        offset = next;
                    }
                }
                catch
                {
                    Dispose();
                    throw;
                }
            }

            public Stream GetStream(string filename)
            {
                Entry entry;
                if (!index.TryGetValue(filename, out entry))
                    return null;

                stream.Seek(entry.Offset, SeekOrigin.Begin);
                var data = stream.ReadBytes((int)entry.Length);
                return new MemoryStream(data);
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
                stream.Dispose();
            }
        }

        bool IPackageLoader.TryParsePackage(Stream s, string filename, FS context, out IReadOnlyPackage package)
        {
            if (!filename.EndsWith(".pak", StringComparison.InvariantCultureIgnoreCase))
            {
                package = null;
                return false;
            }

            package = new PakFile(s, filename);
            return true;
        }
    }
}
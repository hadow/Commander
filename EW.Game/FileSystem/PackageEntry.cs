using System;
using System.IO;
using System.Collections.Generic;

namespace EW.FileSystem
{
    /// <summary>
    /// 
    /// </summary>
    public class PackageEntry
    {
        public readonly uint Hash;
        public readonly uint Offset;
        public readonly uint Length;

        public PackageEntry(uint hash,uint offset,uint length)
        {
            Hash = hash;
            Offset = offset;
            Length = length;
                 
        }

        public PackageEntry(Stream s)
        {
            Hash = s.ReadUInt32();
            Offset = s.ReadUInt32();
            Length = s.ReadUInt32();
        }



    }
}
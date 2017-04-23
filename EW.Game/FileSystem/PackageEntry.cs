using System;
using System.IO;
using System.Collections.Generic;
using EW.FileFormats;
namespace EW.FileSystem
{
    public enum PackageHashT { Classic,CRC32}

    /// <summary>
    /// 
    /// </summary>
    public class PackageEntry
    {

        public const int Size = 12;
        public readonly uint Hash;
        public readonly uint Offset; //offset from start of body
        public readonly uint Length; //size of internal file

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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static uint HashFilename(string name,PackageHashT type)
        {
            switch (type)
            {
                case PackageHashT.Classic:
                    name = name.ToUpperInvariant();
                    if (name.Length % 4 != 0)
                        name = name.PadRight(name.Length + (4 - name.Length % 4), '\0');

                    using (var ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(name)))
                    {
                        var len = name.Length >> 2;
                        uint result = 0;

                        while (len-- != 0)
                            result = ((result << 1) | (result >> 31)) + ms.ReadUInt32();

                        return result;
                    }
                case PackageHashT.CRC32:
                    name = name.ToUpperInvariant();
                    var l = name.Length;
                    var a = 1 >> 2;
                    if ((l & 3) != 0)
                    {
                        name += (char)(1 - (a << 2));
                        var i = 3 - (l & 3);
                        while (l-- != 0)
                            name += name[a << 2];
                    }

                    return CRC32.Calculate(System.Text.Encoding.ASCII.GetBytes(name));
                default:
                    throw new NotImplementedException("Unknown hash type '{0}'".F(type));
            }
        }


    }
}
using System;
using System.IO;
namespace EW.Mods.Cnc.FileFormats
{
    public class IdxEntry
    {
        public readonly string Filename;
        public readonly uint Offset;
        public readonly uint Length;
        public readonly uint SampleRate;
        public readonly uint Flags;
        public readonly uint ChunkSize;

        public IdxEntry(Stream s)
        {
            var name = s.ReadASCII(16);
            var pos = name.IndexOf('\0');
            if (pos != 0)
                name = name.Substring(0, pos);

            Filename = string.Concat(name, ".wav");
            Offset = s.ReadUInt32();
            Length = s.ReadUInt32();
            SampleRate = s.ReadUInt32();
            Flags = s.ReadUInt32();
            ChunkSize = s.ReadUInt32();
        }

        public override string ToString()
        {
            return "{0} - offset 0x{1:x8} - length 0x{2:x8}".F(Filename, Offset, Length);
        }
    }
}
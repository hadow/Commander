﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
namespace EW.Mods.Cnc.FileFormats
{
    public class XccLocalDatabase
    {
        public readonly string[] Entries;
        public XccLocalDatabase(Stream s)
        {
            // Skip unnecessary header data
            s.Seek(48, SeekOrigin.Begin);
            var reader = new BinaryReader(s);
            var count = reader.ReadInt32();
            Entries = new string[count];
            for (var i = 0; i < count; i++)
            {
                var chars = new List<char>();
                char c;
                while ((c = reader.ReadChar()) != 0)
                    chars.Add(c);

                Entries[i] = new string(chars.ToArray());
            }
        }

        public XccLocalDatabase(IEnumerable<string> filenames)
        {
            Entries = filenames.ToArray();
        }

        public byte[] Data()
        {
            var data = new MemoryStream();
            using (var writer = new BinaryWriter(data))
            {
                writer.Write(Encoding.ASCII.GetBytes("XCC by Olaf van der Spek"));
                writer.Write(new byte[] { 0x1A, 0x04, 0x17, 0x27, 0x10, 0x19, 0x80, 0x00 });

                writer.Write(Entries.Aggregate(Entries.Length, (a, b) => a + b.Length) + 52); // Size
                writer.Write(0); // Type
                writer.Write(0); // Version
                writer.Write(0); // Game/Format (0 == TD)
                writer.Write(Entries.Length); // Entries
                foreach (var e in Entries)
                {
                    writer.Write(Encoding.ASCII.GetBytes(e));
                    writer.Write((byte)0);
                }
            }

            return data.ToArray();
        }
    }
}
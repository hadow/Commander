using System;
using System.Collections.Generic;
using System.IO;

namespace EW.FileFormats
{
    public class XccLocalDatabase
    {

        public readonly string[] Entries;

        public XccLocalDatabase(Stream s)
        {
            s.Seek(48, SeekOrigin.Begin);
            var reader = new BinaryReader(s);
            var count = reader.ReadInt32();

            Entries = new string[count];
            for(var i = 0; i < count; i++)
            {
                var chars = new List<char>();
                char c;
                while ((c = reader.ReadChar()) != 0)
                    chars.Add(c);

                Entries[i] = new string(chars.ToArray());
            }

        }


    }
}
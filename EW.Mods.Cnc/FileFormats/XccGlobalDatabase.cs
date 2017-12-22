using System;
using System.Collections.Generic;
using System.IO;
namespace EW.Mods.Cnc.FileFormats
{
    public class XccGlobalDatabase:IDisposable
    {

        public readonly string[] Entries;
        readonly Stream s;

        public XccGlobalDatabase(Stream stream)
        {
            s = stream;

            var entries = new List<string>();
            while (s.Peek() > -1)
            {
                var count = s.ReadUInt32();
                for (var i = 0; i < count; i++)
                {
                    var chars = new List<char>();
                    byte c;

                    //read filename
                    while ((c = s.ReadUInt8()) != 0)
                        chars.Add((char)c);

                    entries.Add(new string(chars.ToArray()));

                    //skip comment
                    while((c = s.ReadUInt8()) != 0) { }
                }

                
                
                
            }

            Entries = entries.ToArray();
        }

        public void Dispose()
        {
            s.Dispose();
        }

    }
}
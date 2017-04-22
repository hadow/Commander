using System;
using System.IO;
using System.Collections.Generic;


namespace EW.FileSystem
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class MixFile:IReadOnlyPackage
    {
        public string Name { get; private set; }

        readonly Dictionary<string, PackageEntry> index;
        public IEnumerable<string> Contents { get { return index.Keys; } }
        readonly long dataStart;

        readonly Stream s;
        readonly FileSystem context;
        public MixFile(FileSystem context,string filename)
        {
            Name = filename;
            this.context = context;

            s = context.Open(filename);
            try
            {
                var isCncMix = s.ReadUInt16() != 0;

                var isEncrypted = false;
                if (!isCncMix)
                    isEncrypted = (s.ReadUInt16() & 0x2) != 0;
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        Dictionary<string,PackageEntry> ParseIndex(Dictionary<uint,PackageEntry> entries)
        {

        }

        /// <summary>
        /// 解析头文件
        /// </summary>
        /// <param name="s"></param>
        /// <param name="offset"></param>
        /// <param name="headerEnd"></param>
        /// <returns></returns>
        static List<PackageEntry> ParseHeader(Stream s,long offset,out long headerEnd)
        {
            s.Seek(offset, SeekOrigin.Begin);
            var numFiles = s.ReadUInt16();          //文件数量
            s.ReadUInt32();                         //Data Size

            var items = new List<PackageEntry>();
            for(var i = 0; i < numFiles; i++)
            {
                items.Add(new PackageEntry(s));
            }

            headerEnd = offset + 6 + numFiles * PackageEntry.Size;
            return items;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public Stream GetContent(PackageEntry entry)
        {
            Stream parentStream;
            var baseOffset = dataStart + entry.Offset;
            var nestedOffset = baseOffset = 

        }


        public Stream GetStream(string filename)
        {
            PackageEntry e;
            if()
        }


        public bool Contains(string filename)
        {
            return index.ContainsKey(filename);
        }

        public void Dispose()
        {
            s.Dispose();
        }

    }
}
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using EW.Primitives;
using EW.FileFormats;

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

                List<PackageEntry> entries;
                if (isEncrypted)
                {
                    long unused;
                    entries = ParseHeader(DecryptHeader(s,4,out dataStart),0,out unused);
                }
                else
                {
                    entries = ParseHeader(s, isCncMix ? 0 : 4, out dataStart);
                }

                index = ParseIndex(entries.ToDictionaryWithConflictLog(x => x.Hash,
                    "{0} ({1} format,Encrypted: {2}, DataStart: {3})".F(filename, isCncMix ? "C&C" : "RA/TS/RA2", isEncrypted, dataStart), null, x => "(offs = {0}, len={1})".F(x.Offset, x.Length)));
                
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// 解析包文件索引
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        Dictionary<string,PackageEntry> ParseIndex(Dictionary<uint,PackageEntry> entries)
        {
            var classicIndex = new Dictionary<string, PackageEntry>();
            var crcIndex = new Dictionary<string, PackageEntry>();

            var allPossibleFilenames = new HashSet<string>();

            var dbNameClassic = PackageEntry.HashFilename("local mix database.dat", PackageHashT.Classic);
            var dbNameCRC = PackageEntry.HashFilename("local mix database.dat", PackageHashT.CRC32);

            foreach(var kv in entries)
            {
                if(kv.Key == dbNameClassic || kv.Key == dbNameCRC)
                {
                    var db = new XccLocalDatabase(GetContent(kv.Value));
                    foreach (var e in db.Entries)
                        allPossibleFilenames.Add(e);

                    break;
                }
            }

            if(context.Exists("global mix database.dat"))
            {
                using(var db = new XccGlobalDatabase(context.Open("global mix database.dat")))
                {
                    foreach (var e in db.Entries)
                        allPossibleFilenames.Add(e);
                }
            }

            foreach(var filename in allPossibleFilenames)
            {
                var classicHash = PackageEntry.HashFilename(filename, PackageHashT.Classic);
                var crcHash = PackageEntry.HashFilename(filename, PackageHashT.CRC32);

                PackageEntry e;

                if (entries.TryGetValue(classicHash, out e))
                    classicIndex.Add(filename, e);

                if (entries.TryGetValue(crcHash, out e))
                    crcIndex.Add(filename, e);
            }

            var bestIndex = crcIndex.Count > classicIndex.Count ? crcIndex : classicIndex;

            var unknown = entries.Count - bestIndex.Count;
            if(unknown>0)
            {
                Android.Util.Log.Debug("debug", "{0}: failed to resolve filenames for {1} unknown hashes".F(Name, unknown));
            }

            return bestIndex;
        }

        static MemoryStream DecryptHeader(Stream s, long offset, out long headerEnd)
        {
            s.Seek(offset, SeekOrigin.Begin);

            // Decrypt blowfish key
            var keyblock = s.ReadBytes(80);
            var blowfishKey = new BlowfishKeyProvider().DecryptKey(keyblock);
            var fish = new Blowfish(blowfishKey);

            // Decrypt first block to work out the header length
            var ms = Decrypt(ReadBlocks(s, offset + 80, 1), fish);
            var numFiles = ms.ReadUInt16();

            // Decrypt the full header - round bytes up to a full block
            var blockCount = (13 + numFiles * PackageEntry.Size) / 8;
            headerEnd = offset + 80 + blockCount * 8;

            return Decrypt(ReadBlocks(s, offset + 80, blockCount), fish);
        }

        static MemoryStream Decrypt(uint[] h, Blowfish fish)
        {
            var decrypted = fish.Decrypt(h);

            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);
            foreach (var t in decrypted)
                writer.Write(t);
            writer.Flush();

            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        static uint[] ReadBlocks(Stream s, long offset, int count)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "Non-negative number required.");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "Non-negative number required.");

            if (offset + (count * 2) > s.Length)
                throw new ArgumentException("Bytes to read {0} and offset {1} greater than stream length {2}.".F(count * 2, offset, s.Length));

            s.Seek(offset, SeekOrigin.Begin);

            // A block is a single encryption unit (represented as two 32-bit integers)
            var ret = new uint[2 * count];
            for (var i = 0; i < ret.Length; i++)
                ret[i] = s.ReadUInt32();

            return ret;
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
            var nestedOffset = baseOffset + SegmentStream.GetOverallNestedOffset(s, out parentStream);

            if(parentStream.GetType() == typeof(FileStream))
            {
                var path = ((FileStream)parentStream).Name;
                return new SegmentStream(File.OpenRead(path), nestedOffset, entry.Length);
            }

            s.Seek(baseOffset, SeekOrigin.Begin);
            return new MemoryStream(s.ReadBytes((int)entry.Length));

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public Stream GetStream(string filename)
        {
            PackageEntry e;
            if (!index.TryGetValue(filename, out e))
                return null;

            return GetContent(e);
        }


        public bool Contains(string filename)
        {
            return index.ContainsKey(filename);
        }

        public IReadOnlyDictionary<string,PackageEntry> Index
        {
            get
            {
                var absoluteIndex = index.ToDictionary(e => e.Key, e => new PackageEntry(e.Value.Hash, (uint)(e.Value.Offset + dataStart), e.Value.Length));
                return new ReadOnlyDictionary<string, PackageEntry>(absoluteIndex);
            }
        }


        public void Dispose()
        {
            s.Dispose();
        }

    }
}
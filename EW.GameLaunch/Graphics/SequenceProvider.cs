using System;
using System.Collections.Generic;
using System.Drawing;
using EW.Primitives;
using EW.FileSystem;
namespace EW.Graphics
{
    using Sequences = IReadOnlyDictionary<string, Lazy<IReadOnlyDictionary<string, ISpriteSequence>>>;
    using UnitSequences = Lazy<IReadOnlyDictionary<string, ISpriteSequence>>;

    /// <summary>
    /// 
    /// </summary>
    public interface ISpriteSequence
    {
        string Name { get; }

        int Start { get; }

        int Length { get; }

        int Stride { get; }

        int Facings { get; }

        int Tick { get; }

        int ZOffset { get; }

        int ShadowStart { get; }

        int ShadowZOffset { get; }

        int[] Frames { get; }

        Rectangle Bounds { get; }

        Sprite GetSprite(int frame);

        Sprite GetSprite(int frame, int facing);

        Sprite GetShadow(int frame, int facing);
    }

    /// <summary>
    /// 精灵序列加载
    /// </summary>
    public interface ISpriteSequenceLoader
    {
        Action<string> OnMissingSpriteError { get; set; }//Sprite 缺失报错

        IReadOnlyDictionary<string, ISpriteSequence> ParseSequences(ModData modData, TileSet tileSet, SpriteCache cache, MiniYamlNode node);
    }


    /// <summary>
    /// 序列对象提供
    /// </summary>
    public class SequenceProvider:IDisposable
    {

        readonly ModData modData;
        readonly TileSet tileSet;
        readonly Lazy<Sequences> sequences;
        readonly Lazy<SpriteCache> spriteCache;

        public SpriteCache SpriteCache { get { return spriteCache.Value; } }

        readonly Dictionary<string, UnitSequences> sequenceCache = new Dictionary<string, UnitSequences>();

        public SequenceProvider(IReadOnlyFileSystem fileSystem,ModData modData,TileSet tileSet,MiniYaml additionalSequences)
        {
            this.modData = modData;
            this.tileSet = tileSet;

            sequences = Exts.Lazy(() => {

                using (new Support.PerfTimer("LoadSequences"))
                    return Load(fileSystem, additionalSequences);
            });

            spriteCache = Exts.Lazy(() =>new SpriteCache(fileSystem,modData.SpriteLoaders,new SheetBuilder(SheetT.Indexed)));

        }


        public IEnumerable<string> Sequences(string unitName)
        {
            UnitSequences unitSeq;
            if (!sequences.Value.TryGetValue(unitName, out unitSeq))
                throw new InvalidOperationException("Unit '{0}' does not have any sequences defined.".F(unitName));
            return unitSeq.Value.Keys;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitName"></param>
        /// <param name="sequenceName"></param>
        /// <returns></returns>
        public ISpriteSequence GetSequence(string unitName,string sequenceName)
        {
            UnitSequences unitSeq;
            if (!sequences.Value.TryGetValue(unitName, out unitSeq))
                throw new InvalidOperationException("Unit '{0}' does not have any sequences defined.".F(unitName));

            ISpriteSequence seq;
            if (!unitSeq.Value.TryGetValue(sequenceName, out seq))
                throw new InvalidOperationException("Unit '{0}' doest not have sequence named '{1}'".F(unitName, sequenceName));

            return seq;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileSystem"></param>
        /// <param name="additionalSequences"></param>
        /// <returns></returns>
        Sequences Load(IReadOnlyFileSystem fileSystem,MiniYaml additionalSequences)
        {
            var nodes = MiniYaml.Load(fileSystem, modData.Manifest.Sequences, additionalSequences);
            var items = new Dictionary<string, UnitSequences>();

            foreach(var n in nodes)
            {
                //Work around the loop closure issue in older versions of C#
                var node = n;
                var key = node.Value.ToLines(node.Key).JoinWith("|");

                UnitSequences t;
                if (sequenceCache.TryGetValue(key, out t))
                    items.Add(node.Key, t);
                else
                {
                    t = Exts.Lazy(() => modData.SpriteSequenceLoader.ParseSequences(modData, tileSet, SpriteCache, node));
                    
                    sequenceCache.Add(key, t);
                    items.Add(node.Key, t);
                }
            }

            return new ReadOnlyDictionary<string, UnitSequences>(items);
        }

        public bool HasSequence(string unitName)
        {
            return sequences.Value.ContainsKey(unitName);
        }

        public bool HasSequence(string unitName,string sequenceName)
        {
            UnitSequences unitSeq;
            if (!sequences.Value.TryGetValue(unitName, out unitSeq))
                throw new InvalidOperationException("Unit '{0}' does not have any sequences defined.".F(unitName));

            return unitSeq.Value.ContainsKey(sequenceName);
        }

        /// <summary>
        /// 预加载
        /// </summary>
        public void PreLoad()
        {
            SpriteCache.SheetBuilder.Current.CreateBuffer();
            foreach(var unitSeq in sequences.Value.Values)
            {
                foreach(var seq in unitSeq.Value.Values)
                {

                }
            }

            SpriteCache.SheetBuilder.Current.ReleaseBuffer();
        }

        public void Dispose()
        {
            if (spriteCache.IsValueCreated)
                spriteCache.Value.SheetBuilder.Dispose();

        }

        //protected override void Dispose(bool disposing)
        //{
        //    base.Dispose(disposing);
        //    if (spriteCache.IsValueCreated)
        //        spriteCache.Value.SheetBuilder.Dispose();
        //}
    }
}
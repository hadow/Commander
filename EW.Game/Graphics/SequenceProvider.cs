using System;
using System.Collections.Generic;
using EW.Primitives;
using EW.FileSystem;
namespace EW.Graphics
{
    using Sequences = EW.Primitives.IReadOnlyDictionary<string, Lazy<EW.Primitives.IReadOnlyDictionary<string, ISpriteSequence>>>;
    using UnitSequences = Lazy<EW.Primitives.IReadOnlyDictionary<string, ISpriteSequence>>;

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

        int[] Frames { get; }

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

        EW.Primitives.IReadOnlyDictionary<string, ISpriteSequence> ParseSequences(ModData modData, TileSet tileSet, SpriteCache cache, MiniYamlNode node);
    }


    /// <summary>
    /// 
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

            spriteCache = Exts.Lazy(() =>new SpriteCache());

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

        public void Dispose() { }
    }
}
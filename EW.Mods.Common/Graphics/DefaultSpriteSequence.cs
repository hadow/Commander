using System;
using System.Collections.Generic;
using System.IO;
using EW.Graphics;
namespace EW.Mods.Common.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    public class DefaultSpriteSequenceLoader:ISpriteSequenceLoader
    {
        public DefaultSpriteSequenceLoader(ModData modData) { }
        public Action<string> OnMissingSpriteError { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="modData"></param>
        /// <param name="tileSet"></param>
        /// <param name="cache"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public EW.Primitives.IReadOnlyDictionary<string,ISpriteSequence> ParseSequences(ModData modData,TileSet tileSet,SpriteCache cache, MiniYamlNode node)
        {
            var sequences = new Dictionary<string, ISpriteSequence>();
            var nodes = node.Value.ToDictionary();

            MiniYaml defaults;
            try
            {
                if(nodes.TryGetValue("Defaults",out defaults))
                {
                    nodes.Remove("Defaults");
                    foreach(var n in nodes)
                    {
                        n.Value.Nodes = MiniYaml.Merge(new[] { defaults.Nodes,n.Value.Nodes });
                        n.Value.Value = n.Value.Value ?? defaults.Value;
                    }
                }
            }
            catch(Exception e)
            {
                throw new InvalidDataException("Error occurred while parsing {0} ".F(node.Key), e);
            }

            foreach(var kvp in nodes)
            {
                using (new Support.PerfTimer("new Sequence(\"{0}\")".F(node.Key), 20))
                {
                    try
                    {
                        sequences.Add(kvp.Key, CreateSequence(modData, tileSet, cache, node.Key, kvp.Key, kvp.Value));
                    }
                    catch(FileNotFoundException ex)
                    {
                        OnMissingSpriteError(ex.Message);
                    }
                   
                }
            }
            //TODO:
            return new EW.Primitives.ReadOnlyDictionary<string, ISpriteSequence>(sequences);
        }

        public virtual ISpriteSequence CreateSequence(ModData modData,TileSet tileSet,SpriteCache cache,string sequence,string animation,MiniYaml info)
        {
            return new DefaultSpriteSequence(modData, tileSet, cache, this, sequence, animation, info);
        }
    }

    public class DefaultSpriteSequence : ISpriteSequence
    {
        readonly Sprite[] sprites;
        public string Name { get; private set; }

        public int Start { get; private set; }

        public int Length { get; private set; }

        public int Stride { get; private set; }

        public int Facings { get; private set; }

        public int Tick { get; private set; }

        public int ZOffset { get; private set; }

        public float ZRamp { get; private set; }

        public int ShadowStart { get; private set; }

        public int ShadowZOffset { get; private set; }

        public int[] Frames { get; private set; }

        public DefaultSpriteSequence(ModData modData,TileSet tileSet,SpriteCache cache,ISpriteSequenceLoader loader,string sequence,string animation,MiniYaml info)
        {

        }

        protected virtual Sprite GetSprite(int start,int frame,int facing)
        {
            return sprites[start];
        }

        public Sprite GetShadow(int frame,int facing)
        {
            return ShadowStart >= 0 ? GetSprite(ShadowStart, frame, facing) : null;
        }
        public Sprite GetSprite(int frame,int facing)
        {
            return GetSprite(Start, frame, facing);
        }

        public Sprite GetSprite(int frame)
        {
            return GetSprite(Start, frame, 0);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EW.Graphics;
using EW.Xna.Platforms;
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
            return new EW.Primitives.ReadOnlyDictionary<string, ISpriteSequence>(sequences);
        }

        public virtual ISpriteSequence CreateSequence(ModData modData,TileSet tileSet,SpriteCache cache,string sequence,string animation,MiniYaml info)
        {
            return new DefaultSpriteSequence(modData, tileSet, cache, this, sequence, animation, info);
        }
    }

    public class DefaultSpriteSequence : ISpriteSequence
    {
        static readonly WDist DefaultShadowSpriteZOffset = new WDist(-5);
        readonly Sprite[] sprites;
        readonly bool reverseFacings, transpose, useClassicFacingFudge;
        protected readonly ISpriteSequenceLoader Loader;
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
            Name = animation;
            Loader = loader;

            var d = info.ToDictionary();

            try
            {
                Start = LoadField(d, "Start", 0);
                ShadowStart = LoadField(d, "ShadowStart", -1);
                ShadowZOffset = LoadField(d, "ShadowZOffset", DefaultShadowSpriteZOffset).Length;
                ZOffset = LoadField(d, "ZOffset", WDist.Zero).Length;
                ZRamp = LoadField(d, "ZRamp", 0);
                Tick = LoadField(d, "Tick", 40);
                transpose = LoadField(d, "Transpose", false);
                Frames = LoadField<int[]>(d, "Frames", null);
                useClassicFacingFudge = LoadField(d, "UseClassicFacingFudge", false);

                var flipX = LoadField(d, "FlipX", false);
                var flipY = LoadField(d, "FlipY", false);

                Facings = LoadField(d, "Facings", 1);
                if (Facings < 0)
                {
                    reverseFacings = true;
                    Facings = -Facings;
                }
                if(useClassicFacingFudge && Facings != 32)
                {
                    throw new InvalidDataException("{0}: Sequence {1}.{2}: UseClassicFacingFudge is only valid for 32 facings".F(info.Nodes[0].Location, sequence, animation));
                }

                var offset = LoadField(d, "Offset", Vector3.Zero);
                var blendmode = LoadField(d, "BlendMode", BlendMode.Alpha);

                MiniYaml combine;
                if(d.TryGetValue("Combine",out combine))
                {
                    var combined = Enumerable.Empty<Sprite>();
                    foreach(var sub in combine.Nodes)
                    {
                        var sd = sub.Value.ToDictionary();

                        var subStart = LoadField(sd, "Start", 0);
                        var subOffset = LoadField(sd, "Offset", Vector3.Zero);
                        var subFlipX = LoadField(sd, "FlipX", false);
                        var subFlipY = LoadField(sd, "FlipY", false);

                        var subSrc = GetSpriteSrc(modData, tileSet, sequence, animation, sub.Key, sd);
                        var subSprites = cache[subSrc].Select(s => new Sprite(s.Sheet, FlipRectangle(s.Bounds, subFlipX, subFlipY), 
                            ZRamp, new Vector3(subFlipX?-s.Offset.X:s.Offset.X,subFlipY?-s.Offset.Y:s.Offset.Y,s.Offset.Z)+subOffset+offset, s.Channel, blendmode));
                    }
                }
            }
            catch(FormatException f)
            {
                throw new FormatException("Failed to parse sequences for {0}.{1} at {2}:\n{3}".F(sequence, animation, info.Nodes[0].Location, f));
            }
        }

        protected static Rectangle FlipRectangle(Rectangle rect,bool flipX,bool flipY)
        {
            var left = flipX ? rect.Right : rect.Left;
            var top = flipY ? rect.Bottom : rect.Top;
            var right = flipX ? rect.Left : rect.Right;
            var bottom = flipY ? rect.Top : rect.Bottom;

            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="d"></param>
        /// <param name="key"></param>
        /// <param name="fallback"></param>
        /// <returns></returns>
        protected static T LoadField<T>(Dictionary<string,MiniYaml> d,string key,T fallback)
        {
            MiniYaml value;
            if (d.TryGetValue(key, out value))
                return FieldLoader.GetValue<T>(key, value.Value);

            return fallback;
        }

        protected virtual string GetSpriteSrc(ModData modData,TileSet tileSet,string sequence,string animation,string sprite,Dictionary<string,MiniYaml> d)
        {
            return sprite ?? sequence;
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
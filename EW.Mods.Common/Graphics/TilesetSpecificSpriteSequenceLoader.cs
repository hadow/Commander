using System;
using System.Collections.Generic;
using System.Linq;
using EW.Graphics;

namespace EW.Mods.Common.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    public class TilesetSpecificSpriteSequenceLoader:DefaultSpriteSequenceLoader
    {
        public readonly string DefaultSpriteExtension = ".shp";
        public readonly Dictionary<string, string> TilesetExtensions = new Dictionary<string, string>();
        public readonly Dictionary<string, string> TilesetCodes = new Dictionary<string, string>();


        public TilesetSpecificSpriteSequenceLoader(ModData modData) : base(modData)
        {
            var metadata = modData.Manifest.Get<SpriteSequenceFormat>().Metadata;
            MiniYaml yaml;

            if (metadata.TryGetValue("DefaultSpriteExtension", out yaml))
            {
                DefaultSpriteExtension = yaml.Value;
            }

            if (metadata.TryGetValue("TilesetExtensions", out yaml))
            {
                TilesetExtensions = yaml.ToDictionary(kv => kv.Value);
            }
            if (metadata.TryGetValue("TilesetCodes", out yaml))
                TilesetCodes = yaml.ToDictionary(kv => kv.Value);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modData"></param>
        /// <param name="tileSet"></param>
        /// <param name="cache"></param>
        /// <param name="sequence"></param>
        /// <param name="animation"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public override ISpriteSequence CreateSequence(ModData modData, TileSet tileSet, SpriteCache cache, string sequence, string animation, MiniYaml info)
        {
            return new TilesetSpecificSpriteSequence(modData, tileSet, cache, this, sequence, animation, info);
        }
    }

    public class TilesetSpecificSpriteSequence : DefaultSpriteSequence
    {
        public TilesetSpecificSpriteSequence(ModData modData,TileSet tileSet,SpriteCache cache,ISpriteSequenceLoader loader,
            string sequence,string animation,MiniYaml info) : base(modData, tileSet, cache, loader, sequence, animation, info) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tileSet"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        string ResolveTilesetId(TileSet tileSet,Dictionary<string,MiniYaml> d)
        {
            var tsId = tileSet.Id;

            MiniYaml yaml;
            if(d.TryGetValue("TilesetOverrides",out yaml))
            {
                var tsNode = yaml.Nodes.FirstOrDefault(n => n.Key == tsId);
                if (tsNode != null)
                    tsId = tsNode.Value.Value;
            }

            return tsId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modData"></param>
        /// <param name="tileSet"></param>
        /// <param name="sequence"></param>
        /// <param name="animation"></param>
        /// <param name="sprite"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        protected override string GetSpriteSrc(ModData modData, TileSet tileSet, string sequence, string animation, string sprite, Dictionary<string, MiniYaml> d)
        {
            var loader = (TilesetSpecificSpriteSequenceLoader)Loader;

            var spriteName = sprite ?? sequence;

            if (LoadField(d, "UseTilesetCode", false))
            {
                string code;
                if(loader.TilesetCodes.TryGetValue(ResolveTilesetId(tileSet,d),out code))
                {
                    spriteName = spriteName.Substring(0, 1) + code + spriteName.Substring(2, spriteName.Length - 2);
                }
            }

            if (LoadField(d, "AddExtension", true))
            {
                var useTilesetExtension = LoadField(d, "UseTilesetExtension", false);

                string tilesetExtension;
                if(useTilesetExtension && loader.TilesetExtensions.TryGetValue(ResolveTilesetId(tileSet,d),out tilesetExtension))
                {
                    return spriteName + tilesetExtension;
                }

                return spriteName + loader.DefaultSpriteExtension;
            }
            Console.WriteLine("Sprite Name:" + spriteName);
            return spriteName;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace EW.Mods.Common.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    public class TilesetSpecificSpriteSequenceLoader:DefaultSpriteSequenceLoader
    {
        public readonly string DefaultSpriteExtension = ".shp";
        public readonly Dictionary<string, string> TilesetExtensions = new Dictionary<string, string>();
        public TilesetSpecificSpriteSequenceLoader(ModData modData) : base(modData)
        {
            var metadata = modData.Manifest.Get<SpriteSequenceFormat>().Metadata;
            MiniYaml yaml;
            if(metadata.TryGetValue("TilesetExtensions",out yaml))
            {
                TilesetExtensions = yaml.ToDictionary(kv => kv.Value);
            }
        }
    }
}
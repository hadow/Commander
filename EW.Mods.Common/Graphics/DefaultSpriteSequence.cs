using System;
using System.Collections.Generic;

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
            //TODO:
            return new EW.Primitives.ReadOnlyDictionary<string, ISpriteSequence>(sequences);
        }
    }
}
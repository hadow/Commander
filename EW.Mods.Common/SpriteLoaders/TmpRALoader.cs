using System;
using System.IO;
using EW.Graphics;
namespace EW.Mods.Common.SpriteLoaders
{
    public class TmpRALoader:ISpriteLoader
    {
        public bool TryParseSprite(Stream s, out ISpriteFrame[] frames)
        {
            frames = null;
            return true;
        }
    }
}
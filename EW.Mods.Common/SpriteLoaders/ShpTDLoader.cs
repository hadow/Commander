using System;
using EW.Graphics;
using System.IO;
namespace EW.Mods.Common.SpriteLoaders
{
    public class ShpTDLoader:ISpriteLoader
    {

        public bool TryParseSprite(Stream s,out ISpriteFrame[] frames)
        {
            frames = null;
            return true;
        }
    }
}
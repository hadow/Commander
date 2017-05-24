using System;
using System.Collections.Generic;
using EW.Graphics;

namespace EW.Mods.Common.Traits
{
    class PaletteFromRGBAInfo : ITraitInfo
    {
        public readonly string Name = null;

        public readonly string Tileset = null;

        public readonly int R = 0;

        public readonly int G = 0;

        public readonly int B = 0;

        public readonly int A = 255;

        public readonly bool AllowModifiers = true;

        public object Create(ActorInitializer init) { return new PaletteFromRGBA(init.World,this); }
    }
    class PaletteFromRGBA:ILoadsPalettes
    {
        readonly World world;
        readonly PaletteFromRGBAInfo info;

        public PaletteFromRGBA(World world,PaletteFromRGBAInfo info)
        {
            this.world = world;
            this.info = info;
        }

        public void LoadPalettes(WorldRenderer wr)
        {

        }
    }
}
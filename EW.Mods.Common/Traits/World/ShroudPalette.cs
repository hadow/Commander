using System;
using System.Linq;
using EW.Graphics;
using EW.Xna.Platforms;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    class ShroudPaletteInfo : ITraitInfo
    {
        [FieldLoader.Require,PaletteDefinition]
        public readonly string Name = "shroud";


        public readonly bool Fog = false;

        public object Create(ActorInitializer init)
        {
            return new ShroudPalette(this);
        }
    }
    class ShroudPalette:ILoadsPalettes
    {
        readonly ShroudPaletteInfo info;

        static readonly Color[] Fog = new[]
        {
            Color.FromArgb(0,0,0,0),
            Color.Green,
            Color.Blue,
            Color.Yellow,
            Color.FromArgb(128,0,0,0),
            Color.FromArgb(96,0,0,0),
            Color.FromArgb(64,0,0,0),
            Color.FromArgb(32,0,0,0),
        };


        static readonly Color[] Shroud = new[]
        {
            Color.FromArgb(0,0,0,0),
            Color.Green,
            Color.Blue,
            Color.Yellow,
            Color.Black,
            Color.FromArgb(160,0,0,0),
            Color.FromArgb(128,0,0,0),
            Color.FromArgb(64,0,0,0)
    
        };
        public ShroudPalette(ShroudPaletteInfo info)
        {
            this.info = info;
        }
        
        public void LoadPalettes(WorldRenderer wr)
        {
            var c = info.Fog ? Fog : Shroud;
            wr.AddPalette(info.Name, new ImmutablePalette(Enumerable.Range(0, Palette.Size).Select(i => (uint)c[i % 8].ToArgb())));
        }



    }
}
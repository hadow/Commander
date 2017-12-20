using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Graphics;
using EW.OpenGLES;
namespace EW.Mods.Cnc.Traits
{
    class TSShroudPaletteInfo:ITraitInfo
    {
        [FieldLoader.Require,PaletteDefinition]
        public readonly string Name = "shroud";

        public object Create(ActorInitializer init) { return new TSShroudPalette(this); }
    }

    class TSShroudPalette:ILoadsPalettes,IProvidesAssetBrowserPalettes
    {
        readonly TSShroudPaletteInfo info;
        public TSShroudPalette(TSShroudPaletteInfo info)
        {
            this.info = info;
        }


        public void LoadPalettes(WorldRenderer wr)
        {
            
            Func<int, uint> makeColor = i =>
             {
                 if (i < 128)
                     return (uint)(Int2.Lerp(255, 0, i, 127) << 24);
                 return 0;
             };

            wr.AddPalette(info.Name, new ImmutablePalette(Enumerable.Range(0, Palette.Size).Select(i => makeColor(i))));
        }

        public IEnumerable<string> PaletteNames { get { yield return info.Name; } }

    }
}
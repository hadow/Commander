using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    class PaletteFromPlayerPaletteWithAlphaInfo:ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new PaletteFromPlayerPaletteWithAlpha(this);
        }

    }

    class PaletteFromPlayerPaletteWithAlpha
    {
        readonly PaletteFromPlayerPaletteWithAlphaInfo info;
        public PaletteFromPlayerPaletteWithAlpha(PaletteFromPlayerPaletteWithAlphaInfo info)
        {
            this.info = info;
        }
    }
}
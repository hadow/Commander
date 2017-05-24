using System;
using System.Collections.Generic;

namespace EW.Mods.Common.Traits
{
    class ShroudPaletteInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new ShroudPaletteInfo();
        }
    }
    class ShroudPalette
    {
    }
}
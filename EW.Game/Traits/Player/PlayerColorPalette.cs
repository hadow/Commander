using System;
using System.Collections.Generic;

namespace EW.Traits
{
    public class PlayerColorPaletteInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new PlayerColorPalette();
        }
    }
    public class PlayerColorPalette
    {
    }
}
using System;

namespace EW.Traits
{
    public class PlayerHighlightPaletteInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new PlayerHighlightPalette();
        }
    }
    public class PlayerHighlightPalette
    {
    }
}
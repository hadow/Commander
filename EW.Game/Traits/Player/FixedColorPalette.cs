using System;

namespace EW.Traits
{
    public class FixedColorPaletteInfo: ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new FixedColorPalette();
        }
    }
    public class FixedColorPalette
    {


    }
}
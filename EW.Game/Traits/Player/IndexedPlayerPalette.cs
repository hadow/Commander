using System;
using System.Collections.Generic;


namespace EW.Traits
{

    public class IndexedPlayerPaletteInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new IndexedPlayerPalette(); }
    }
    class IndexedPlayerPalette
    {
    }
}
using System;
using System.Collections.Generic;


namespace EW.Mods.Common.Traits
{



    public class SelectionDecorationsInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new SelectionDecorations(); }
    }
    public class SelectionDecorations
    {
    }
}
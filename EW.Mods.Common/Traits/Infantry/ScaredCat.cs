using System;
using System.Collections.Generic;


namespace EW.Mods.Common.Traits
{

    class ScaredCatInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new ScaredCat(); }
    }
    class ScaredCat
    {
    }
}
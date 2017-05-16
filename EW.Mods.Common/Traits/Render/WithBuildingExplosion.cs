using System;
using System.Collections.Generic;

namespace EW.Mods.Common.Traits
{

    class WithBuildingExplosionInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new WithBuildingExplosion(); }
    }

    class WithBuildingExplosion
    {
    }
}
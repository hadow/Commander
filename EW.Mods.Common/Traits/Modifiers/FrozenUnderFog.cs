using System;
using System.Collections.Generic;

namespace EW.Mods.Common.Traits
{


    public class FrozenUnderFogInfo : ITraitInfo, Requires<BuildingInfo>
    {
        public object Create(ActorInitializer init) { return new FrozenUnderFog(); }
    }

    public class FrozenUnderFog
    {
    }
}
using System;
using System.Collections.Generic;

namespace EW.Mods.Common.Traits
{
    class ContrailInfo : ITraitInfo, Requires<BodyOrientationInfo>
    {
        public object Create(ActorInitializer init) { return new Contrail(); }
    }
    class Contrail
    {
    }
}
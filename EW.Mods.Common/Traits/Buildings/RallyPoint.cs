using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    class RallyPointInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new RallyPoint(); }
    }
    class RallyPoint
    {
    }
}
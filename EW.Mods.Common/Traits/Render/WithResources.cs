using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    class WithResourcesInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new WithResources(); }
    }
    class WithResources
    {
    }
}
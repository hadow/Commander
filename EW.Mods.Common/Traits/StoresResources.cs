using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    class StoresResourcesInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new StoresResources(); }
    }
    class StoresResources
    {
    }
}
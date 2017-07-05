using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class ResourceStorageWarningInfo : ITraitInfo, Requires<PlayerResourcesInfo>
    {
        public object Create(ActorInitializer init) { return new ResourceStorageWarning(); }
    }
    class ResourceStorageWarning
    {
    }
}
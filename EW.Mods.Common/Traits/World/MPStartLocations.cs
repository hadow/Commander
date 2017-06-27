using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class MPStartLocationsInfo : ITraitInfo
    {
        public virtual object Create(ActorInitializer init) { return new MPStartLocations(); }
    }

    public class MPStartLocations
    {
    }
}
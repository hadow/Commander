using System;

namespace EW.Mods.Common.Traits
{

    public class ResourceLayerInfo : ITraitInfo, Requires<BuildingInfluenceInfo>
    {
        public object Create(ActorInitializer init) { return new ResourceLayer(); }
    }

    public class ResourceLayer
    {
    }
}
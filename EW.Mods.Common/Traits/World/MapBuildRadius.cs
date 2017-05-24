using System;

namespace EW.Mods.Common.Traits
{

    public class MapBuildRadiusInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new MapBuildRadius(); }
    }

    public class MapBuildRadius
    {
    }
}
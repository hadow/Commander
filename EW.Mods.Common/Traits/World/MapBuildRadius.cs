using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class MapBuildRadiusInfo : ITraitInfo
    {
        public readonly bool AllyBuildRadiusEnabled = true;

        public readonly bool AllyBuildRadiusLocked = false;




        public object Create(ActorInitializer init) { return new MapBuildRadius(); }
    }

    public class MapBuildRadius
    {

        public bool AllyBuildRadiusEnabled { get; private set; }
    }
}
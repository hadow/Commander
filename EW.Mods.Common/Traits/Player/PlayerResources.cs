using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class PlayerResourcesInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new PlayerResources(); }
    }
    class PlayerResources
    {
    }
}
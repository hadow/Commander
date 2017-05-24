using System;
namespace EW.Mods.Common.Traits
{

    public class MapOptionsInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new MapOptions(); }
    }

    public class MapOptions
    {
    }
}
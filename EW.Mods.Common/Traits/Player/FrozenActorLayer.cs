using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class FrozenActorLayerInfo : Requires<ShroudInfo>, ITraitInfo
    {
        public object Create(ActorInitializer init) { return new FrozenActorLayer(); }
    }
    class FrozenActorLayer
    {
    }
}
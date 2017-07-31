using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    class WithSiloAnimationInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new WithSiloAnimation(); }
    }
    class WithSiloAnimation
    {
    }
}
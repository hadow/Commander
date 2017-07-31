using System;
using EW.Traits;

namespace EW.Mods.Common.Traits.Render
{


    public class WithBuildingPlacedAnimationInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new WithBuildingPlacedAnimation(); }
    }
    class WithBuildingPlacedAnimation
    {
    }
}
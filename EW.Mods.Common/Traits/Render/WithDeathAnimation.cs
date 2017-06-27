using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class WithDeathAnimationInfo : ITraitInfo,Requires<RenderSpritesInfo>
    {
        public object Create(ActorInitializer init)
        {
            return new WithDeathAnimation();
        }
    }
    public class WithDeathAnimation
    {
    }
}
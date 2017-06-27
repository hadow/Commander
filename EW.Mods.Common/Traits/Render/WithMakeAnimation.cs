using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class WithMakeAnimationInfo : ITraitInfo,Requires<WithSpriteBodyInfo>
    {
        public object Create(ActorInitializer init) { return new WithMakeAnimation(); }
    }

    public class WithMakeAnimation
    {
    }
}
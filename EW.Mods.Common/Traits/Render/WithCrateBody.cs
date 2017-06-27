using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    class WithCrateBodyInfo : ITraitInfo, Requires<RenderSpritesInfo>
    {
        public object Create(ActorInitializer init)
        {
            return new WithCrateBody();
        }
    }

    class WithCrateBody
    {
    }
}
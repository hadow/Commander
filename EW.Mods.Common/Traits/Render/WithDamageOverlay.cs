﻿using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class WithDamageOverlayInfo : ITraitInfo,Requires<RenderSpritesInfo>
    {

        public object Create(ActorInitializer init)
        {
            return new WithDamageOverlay();
        }
    }
    public class WithDamageOverlay
    {
    }
}
﻿using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class WithSpriteControlGroupDecorationInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new WithSpriteControlGroupDecoration();
        }

    }

    public class WithSpriteControlGroupDecoration
    {



    }
}
﻿using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class GuardInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new Guard();
        }
    }
    public class Guard
    {
    }
}
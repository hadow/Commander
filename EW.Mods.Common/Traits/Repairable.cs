﻿using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class RepairableInfo : ITraitInfo
    {
        public virtual object Create(ActorInitializer init)
        {
            return new Repairable();
        }
    }
    public class Repairable
    {
    }
}
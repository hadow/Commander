﻿using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class ScalePowerWithHealthInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new ScalePowerWithHealth(); }
    }

    public class ScalePowerWithHealth
    {
    }
}
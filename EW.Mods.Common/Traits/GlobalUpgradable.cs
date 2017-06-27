using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class GlobalUpgradableInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new GlobalUpgradable();
        }
    }
    public class GlobalUpgradable
    {
    }
}
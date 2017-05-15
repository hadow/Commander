using System;
using System.Collections.Generic;


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
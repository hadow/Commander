using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public abstract class UpgradableMultiplierInfo : ITraitInfo
    {
        public abstract object Create(ActorInitializer init);
    }

    public class UpgradableMultiplier
    {
    }
}
using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class ReloadDelayMultiplierInfo : UpgradeMultiplierTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            throw new NotImplementedException();
        }
    }
    public class ReloadDelayMultiplier:UpgradeMultiplierTrait,IReloadModifier
    {
        public ReloadDelayMultiplier(ReloadDelayMultiplierInfo info,string actorType) : base(info, "ReloadDelayMultiplier", actorType) { }

        public int GetReloadModifier() { return GetModifier(); }

    }
}
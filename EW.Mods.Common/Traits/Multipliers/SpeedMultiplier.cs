using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class SpeedMultiplierInfo : UpgradeMultiplierTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new SpeedMultiplier(this, init.Self.Info.Name);
        }
    }
    public class SpeedMultiplier:UpgradeMultiplierTrait,ISpeedModifier
    {
        public SpeedMultiplier(SpeedMultiplierInfo info,string actorType) : base(info, "SpeedMultiplier", actorType) { }

        public int GetSpeedModifier() { return GetModifier(); }
    }
}
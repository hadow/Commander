using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class InaccuracyMultiplierInfo : UpgradeMultiplierTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new InaccuracyMultiplier(this, init.Self.Info.Name);
        }
    }
    public class InaccuracyMultiplier:UpgradeMultiplierTrait
    {
        public InaccuracyMultiplier(InaccuracyMultiplierInfo info,string actorType) : base(info, "InaccuracyMultiplier", actorType) { }
    }
}
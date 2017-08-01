using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class FirepowerMultiplierInfo : UpgradeMultiplierTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new FirepowerMultiplier(this, init.Self.Info.Name);
        }
    }

    public class FirepowerMultiplier:UpgradeMultiplierTrait
    {

        public FirepowerMultiplier(FirepowerMultiplierInfo info,string actorType) : base(info, "FirepowerMultiplier", actorType) { }
    }
}
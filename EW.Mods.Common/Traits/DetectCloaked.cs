using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class DetectCloakedInfo : UpgradableTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new DetectCloaked(this);
        }
    }
    public class DetectCloaked:UpgradableTrait<DetectCloakedInfo>
    {
        public DetectCloaked(DetectCloakedInfo info) : base(info) { }

    }
}
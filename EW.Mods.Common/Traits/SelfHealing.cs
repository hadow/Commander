using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    class SelfHealingInfo : UpgradableTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new SelfHealing(init.Self, this);
        }
    }
    class SelfHealing:UpgradableTrait<SelfHealingInfo>,ITick
    {
        public SelfHealing(Actor self,SelfHealingInfo info) : base(info) { }
        public void Tick(Actor self) { }
    }
}
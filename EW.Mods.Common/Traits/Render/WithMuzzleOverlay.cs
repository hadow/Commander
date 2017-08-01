using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{

    class WithMuzzleOverlayInfo : UpgradableTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new WithMuzzleOverlay(init.Self, this);
        }
    }
    class WithMuzzleOverlay:UpgradableTrait<WithMuzzleOverlayInfo>
    {

        public WithMuzzleOverlay(Actor self,WithMuzzleOverlayInfo info) : base(info) { }
    }
}
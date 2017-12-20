using System;
using EW.Traits;
namespace EW.Mods.Common.Traits.Render
{

    class WithRepairOverlayInfo : PausableConditionalTraitInfo,Requires<BodyOrientationInfo>,Requires<RenderSpritesInfo>
    {
        public override object Create(ActorInitializer init)
        {
            return new WithRepairOverlay();
        }
    }
    class WithRepairOverlay
    {
    }
}
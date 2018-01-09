using System;
using EW.Graphics;
using EW.Traits;
namespace EW.Mods.Common.Traits.Render
{
    public class WithNukeLaunchOverlayInfo:ConditionalTraitInfo,Requires<RenderSpritesInfo>,Requires<BodyOrientationInfo>
    {
        public override object Create(ActorInitializer init)
        {
            return new WithNukeLaunchOverlay(init.Self, this);
        }

    }

    public class WithNukeLaunchOverlay : ConditionalTrait<WithNukeLaunchOverlayInfo>
    {
        public  WithNukeLaunchOverlay(Actor self,WithNukeLaunchOverlayInfo info) : base(info)
        {

        }
    }

}
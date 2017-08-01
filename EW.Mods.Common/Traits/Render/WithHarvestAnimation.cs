using System;
using EW.Traits;
namespace EW.Mods.Common.Traits.Render
{


    public class WithHarvestAnimationInfo : ITraitInfo, Requires<WithSpriteBodyInfo>, Requires<HarvesterInfo>
    {
        public object Create(ActorInitializer init) { return new WithHarvestAnimation(init, this); }
    }
    public class WithHarvestAnimation
    {

        public WithHarvestAnimation(ActorInitializer init,WithHarvestAnimationInfo info)
        {

        }
    }
}
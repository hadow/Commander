using System;
using EW.Traits;
namespace EW.Mods.Common.Traits.Render
{
    public class WithIdleAnimationInfo : ConditionalTraitInfo, Requires<WithSpriteBodyInfo>
    {
        public override object Create(ActorInitializer init)
        {
            return new WithIdleAnimation(init.Self, this);
        }
    }
    public class WithIdleAnimation:ConditionalTrait<WithIdleAnimationInfo>
    {

        public WithIdleAnimation(Actor self,WithIdleAnimationInfo info) : base(info)
        {

        }
    }
}
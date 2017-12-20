using System;
using EW.Traits;
namespace EW.Mods.Common.Traits.Render
{
    public class WithAttackAnimationInfo:ConditionalTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new WithAttackAnimation();
        }

    }

    public class WithAttackAnimation
    {

    }
}
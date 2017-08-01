using System;
using EW.Traits;
namespace EW.Mods.Common.Traits.Render
{

    public class WithMoveAnimationInfo : ITraitInfo, Requires<WithSpriteBodyInfo>, Requires<IMoveInfo>
    {
        public object Create(ActorInitializer init) { return new WithMoveAnimation(init,this); }
    }

    public class WithMoveAnimation:ITick
    {
        public WithMoveAnimation(ActorInitializer init,WithMoveAnimationInfo info) { }
        public void Tick(Actor self) { }

    }
}
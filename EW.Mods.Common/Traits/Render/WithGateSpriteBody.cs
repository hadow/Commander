using System;
using EW.Traits;

namespace EW.Mods.Common.Traits.Render
{
    class WithGateSpriteBodyInfo:WithSpriteBodyInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new WithGateSpriteBody(init, this);
        }

    }

    class WithGateSpriteBody:WithSpriteBody
    {
        public WithGateSpriteBody(ActorInitializer init,WithGateSpriteBodyInfo info) : base(init, info, () => 0)
        {

        }
    }
}
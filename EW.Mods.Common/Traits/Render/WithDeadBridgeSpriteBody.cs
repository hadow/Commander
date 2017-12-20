using System;
using EW.Traits;

namespace EW.Mods.Common.Traits.Render
{
    class WithDeadBridgeSpriteBodyInfo:WithSpriteBodyInfo
    {

        public override object Create(ActorInitializer init)
        {
            return new WithDeadBridgeSpriteBody(init, this);
        }
    }

    class WithDeadBridgeSpriteBody : WithSpriteBody
    {
        public WithDeadBridgeSpriteBody(ActorInitializer init,WithDeadBridgeSpriteBodyInfo info) : base(init, info, () => 0)
        {

        }
    }

}
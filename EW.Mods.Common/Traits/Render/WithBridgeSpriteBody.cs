using System;
using EW.Traits;

namespace EW.Mods.Common.Traits.Render
{
    class WithBridgeSpriteBodyInfo:WithSpriteBodyInfo
    {

        public override object Create(ActorInitializer init)
        {
            return new WithBridgeSpriteBody(init, this);
        }
    }

    class WithBridgeSpriteBody:WithSpriteBody
    {
        public WithBridgeSpriteBody(ActorInitializer init,WithBridgeSpriteBodyInfo info):base(init,info,()=>0)
        {

        }
    }


}
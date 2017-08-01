using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class WithSpriteBodyInfo : UpgradableTraitInfo
    {

        public override object Create(ActorInitializer init)
        {
            return new WithSpriteBody(init, this);
        }
    }

    public class WithSpriteBody:UpgradableTrait<WithSpriteBodyInfo>
    {
        public WithSpriteBody(ActorInitializer init,WithSpriteBodyInfo info) : this(init, info, () => 0) { }

        protected WithSpriteBody(ActorInitializer init,WithSpriteBodyInfo info,Func<int> baseFacing) : base(info) { }
    }
}
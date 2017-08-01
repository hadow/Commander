using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class WithShadowInfo : UpgradableTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new WithShadow(this);
        }
    }
    public class WithShadow:UpgradableTrait<WithShadowInfo>
    {

        public WithShadow(WithShadowInfo info) : base(info) { }
    }
}
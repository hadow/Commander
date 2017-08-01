using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class WithDecorationInfo : UpgradableTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new WithDecoration(init.Self, this);
        }
    }

    public class WithDecoration:UpgradableTrait<WithDecorationInfo>
    {
        public WithDecoration(Actor self,WithDecorationInfo info) : base(info) { }
    }
}
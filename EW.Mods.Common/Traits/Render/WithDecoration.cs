using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class WithDecorationInfo : ConditionalTraitInfo,Requires<IDecorationBoundsInfo>
    {
        public override object Create(ActorInitializer init)
        {
            return new WithDecoration(init.Self, this);
        }
    }

    public class WithDecoration:ConditionalTrait<WithDecorationInfo>
    {
        public WithDecoration(Actor self,WithDecorationInfo info) : base(info) { }
    }
}
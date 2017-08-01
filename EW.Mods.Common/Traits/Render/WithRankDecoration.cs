using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class WithRankDecorationInfo : WithDecorationInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new WithRankDecoration(init.Self, this);

        }
    }

    public class WithRankDecoration:WithDecoration
    {
        public WithRankDecoration(Actor self,WithRankDecorationInfo info) : base(self, info) { }
    }
}
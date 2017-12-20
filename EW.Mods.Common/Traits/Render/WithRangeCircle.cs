using System;
using EW.Traits;
namespace EW.Mods.Common.Traits.Render
{

    class WithRangeCircleInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new WithRangeCircle(init.Self, this); }
    }
    class WithRangeCircle
    {

        public WithRangeCircle(Actor self,WithRangeCircleInfo info) { }
    }
}
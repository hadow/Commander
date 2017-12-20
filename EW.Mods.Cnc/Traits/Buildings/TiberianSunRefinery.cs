using System;
using EW.Traits;
using EW.Mods.Common.Traits;
namespace EW.Mods.Cnc.Traits
{
    class TiberianSunRefineryInfo:RefineryInfo
    {

        public override object Create(ActorInitializer init)
        {
            return new TiberianSunRefinery(init.Self, this);
        }
    }

    class TiberianSunRefinery:Refinery
    {

        public TiberianSunRefinery(Actor self ,RefineryInfo info) : base(self, info)
        {

        }
    }
}
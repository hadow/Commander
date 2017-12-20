using System;
using EW.Traits;

namespace EW.Mods.Common.Traits.Render
{
    class TimedConditionBarInfo:ITraitInfo,Requires<ConditionManagerInfo>
    {

        public object Create(ActorInitializer init)
        {
            return new TimedConditionBar();
        }
    }

    class TimedConditionBar
    {

    }
}
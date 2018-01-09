using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class GrantConditionOnPowerStateInfo:ConditionalTraitInfo
    {

        public override object Create(ActorInitializer init)
        {
            return new GrantConditionOnPowerState();
        }
    }


    public class GrantConditionOnPowerState
    {

    }
}
using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class ToggleConditionOnOrderInfo:PausableConditionalTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new ToggleConditionOnOrder();
        }

    }

    public class ToggleConditionOnOrder
    {

    }
}
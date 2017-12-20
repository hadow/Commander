using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class ProximityExternalConditionInfo:ConditionalTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new ProximityExternalCondition(init.Self, this);
        }

    }

    public class ProximityExternalCondition:ConditionalTrait<ProximityExternalConditionInfo>
    {
        public ProximityExternalCondition(Actor self,ProximityExternalConditionInfo info):base(info)
        {

        }
    }
}
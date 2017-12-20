using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class AffectedByPowerOutageInfo:ConditionalTraitInfo
    {

        public override object Create(ActorInitializer init)
        {
            return new AffectedByPowerOutage(init.Self, this);
        }
    }

    public class AffectedByPowerOutage : ConditionalTrait<AffectedByPowerOutageInfo>
    {
        public AffectedByPowerOutage(Actor self,AffectedByPowerOutageInfo info) : base(info)
        {

        }
    }
}
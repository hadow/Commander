using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class PowerMultiplierInfo : ConditionalTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new PowerMultiplier(init.Self, this);
        }
    }
    public class PowerMultiplier:ConditionalTrait<PowerMultiplierInfo>
    {

        public PowerMultiplier(Actor self,PowerMultiplierInfo info) : base(info)
        {

        }
    }
}
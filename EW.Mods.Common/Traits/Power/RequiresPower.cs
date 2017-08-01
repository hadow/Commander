using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    class RequiresPowerInfo : UpgradableTraitInfo, ITraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new RequiresPower(init.Self, this);
        }
    }
    class RequiresPower:UpgradableTrait<RequiresPowerInfo>
    {


        public RequiresPower(Actor self,RequiresPowerInfo info) : base(info)
        {

        }
    }
}
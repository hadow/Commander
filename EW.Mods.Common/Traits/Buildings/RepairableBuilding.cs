using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class RepairableBuildingInfo : UpgradableTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new RepairableBuilding(init.Self, this);
        }
    }

    public class RepairableBuilding:UpgradableTrait<RepairableBuildingInfo>
    {
        public RepairableBuilding(Actor self,RepairableBuildingInfo info) : base(info) { }
    }
}
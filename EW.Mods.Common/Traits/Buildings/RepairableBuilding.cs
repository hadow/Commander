using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    /// <summary>
    /// Building can be repaired by the repair button
    /// </summary>
    public class RepairableBuildingInfo : ConditionalTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new RepairableBuilding(init.Self, this);
        }
    }

    public class RepairableBuilding:ConditionalTrait<RepairableBuildingInfo>,ITick
    {
        public RepairableBuilding(Actor self,RepairableBuildingInfo info) : base(info) { }



        public void Tick(Actor self){
            
        }
    }
}
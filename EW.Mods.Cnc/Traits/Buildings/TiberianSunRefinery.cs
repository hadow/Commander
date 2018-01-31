using System;
using EW.Traits;
using EW.Mods.Common.Traits;
using EW.Activities;
using EW.Mods.Cnc.Activities;
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

        public TiberianSunRefinery(Actor self ,RefineryInfo info) : base(self, info) { }

        public override Activity DockSequence(Actor harv, Actor self)
        {
            return new VoxelHarvesterDockSequence(harv, self, DeliveryAngle, IsDragRequired, DragOffset, DragLength);
        }
    }
}
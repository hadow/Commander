using System;
using System.Drawing;
using EW.Activities;
using EW.Mods.Common.Traits;
using EW.Traits;
namespace EW.Mods.Common.Activities
{
    /// <summary>
    /// 交付资源
    /// </summary>
    public class DeliverResources:Activity
    {
        const int NextChooseTime = 100;

        readonly IMove movement;
        readonly Harvester harv;

        bool isDocking;
        int chosenTicks;

        public DeliverResources(Actor self)
        {
            movement = self.Trait<IMove>();
            harv = self.Trait<Harvester>();
            IsInterruptible = false;

        }


        public override Activity Tick(Actor self)
        {
            if (NextInQueue != null)
                return NextInQueue;

            if(harv.OwnerLinkedProc == null || !harv.OwnerLinkedProc.IsInWorld)
            {
                harv.OwnerLinkedProc = null;
                if(self.World.WorldTick - chosenTicks > NextChooseTime)
                {
                    harv.ChooseNewProc(self, null);
                    chosenTicks = self.World.WorldTick;
                }
            }
            else
            {
                harv.LinkProc(self, harv.OwnerLinkedProc);
            }


            if (harv.LinkedProc == null || !harv.LinkedProc.IsInWorld)
                harv.ChooseNewProc(self, null);

            if (harv.LinkedProc == null)
                return ActivityUtils.SequenceActivities(new Wait(harv.Info.SearchForDeliveryBuildingDelay), this);

            var proc = harv.LinkedProc;
            var iao = proc.Trait<IAcceptResources>();

            self.SetTargetLine(Target.FromActor(proc), Color.Green, false);

            if(self.Location != proc.Location + iao.DeliveryOffset)
            {
                return ActivityUtils.SequenceActivities(movement.MoveTo(proc.Location + iao.DeliveryOffset, 0), this);
            }

            if (!isDocking)
            {
                isDocking = true;
                iao.OnDock(self, this);
            }

            return ActivityUtils.SequenceActivities(new Wait(10), this);

        }
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using EW.Activities;
using EW.Mods.Common.Traits;
using EW.Traits;
using EW.Mods.Common.Pathfinder;

namespace EW.Mods.Common.Activities
{
    public class FindResources:Activity
    {
        readonly Harvester harv;
        readonly HarvesterInfo harvInfo;
        readonly Mobile mobile;
        readonly MobileInfo mobileInfo;
        readonly ResourceClaimLayer claimLayer;
        readonly IPathFinder pathFinder;
        readonly DomainIndex domainIndex;

        CPos? avoidCell;

        public FindResources(Actor self)
        {
            harv = self.Trait<Harvester>();
            harvInfo = self.Info.TraitInfo<HarvesterInfo>();
            mobile = self.Trait<Mobile>();
            mobileInfo = self.Info.TraitInfo<MobileInfo>();
            claimLayer = self.World.WorldActor.Trait<ResourceClaimLayer>();
            pathFinder = self.World.WorldActor.Trait<IPathFinder>();
            domainIndex = self.World.WorldActor.Trait<DomainIndex>();


        }

        public FindResources(Actor self,CPos avoidCell) : this(self)
        {
            this.avoidCell = avoidCell;
        }


        public override Activity Tick(Actor self)
        {
            if (IsCanceled)
                return NextActivity;

            if (NextInQueue != null)
                return NextInQueue;

            var deliver = new DeliverResources(self);

            if (harv.IsFull)
                return ActivityUtils.SequenceActivities(deliver, NextActivity);

            var closestHarvestablePosition = ClosestHarvestablePos(self);

            if (!closestHarvestablePosition.HasValue)
            {
                if (!harv.IsEmpty)
                    return deliver;

                harv.LastSearchFailed = true;

                var unblockCell = harv.LastHarvestedCell ?? (self.Location + harvInfo.UnblockCell);
                var moveTo = mobile.NearestMoveableCell(unblockCell, 2, 5);
                self.QueueActivity(mobile.MoveTo(moveTo, 1));
                self.SetTargetLine(Target.FromCell(self.World, moveTo), Color.Gray, false);



                var randFrames = self.World.SharedRandom.Next(100, 175);

                //Avoid creating an activity cycle
                var next = NextInQueue;
                NextInQueue = null;
                return ActivityUtils.SequenceActivities(next, new Wait(randFrames), this);
            }
            else
            {
                //Attempt to claim the target cell
                if (!claimLayer.TryClaimCell(self, closestHarvestablePosition.Value))
                    return ActivityUtils.SequenceActivities(new Wait(25), this);

                harv.LastSearchFailed = false;

                if (!harv.LastOrderLocation.HasValue)
                    harv.LastOrderLocation = closestHarvestablePosition;

                self.SetTargetLine(Target.FromCell(self.World, closestHarvestablePosition.Value), Color.Red, false);

                return ActivityUtils.SequenceActivities(mobile.MoveTo(closestHarvestablePosition.Value, 1), new HarvestResource(self), this);
            }
        }


        CPos? ClosestHarvestablePos(Actor self)
        {

            return null;
        }
    }
}
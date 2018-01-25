using System;
using EW.Activities;
using EW.Mods.Common.Traits;
using EW.Traits;

namespace EW.Mods.Common.Activities
{
    public class HarvestResource:Activity
    {


        readonly Harvester harv;
        readonly HarvesterInfo harvInfo;
        readonly IFacing facing;
        readonly ResourceClaimLayer claimLayer;
        readonly ResourceLayer resLayer;
        readonly BodyOrientation body;

        public HarvestResource(Actor self)
        {
            harv = self.Trait<Harvester>();
            harvInfo = self.Info.TraitInfo<HarvesterInfo>();
            facing = self.Trait<IFacing>();
            body = self.Trait<BodyOrientation>();
            claimLayer = self.World.WorldActor.Trait<ResourceClaimLayer>();
            resLayer = self.World.WorldActor.Trait<ResourceLayer>();

        }


        public override Activity Tick(Actor self)
        {
            if (IsCanceled)
            {
                claimLayer.RemoveClaim(self);
                return NextActivity;
            }

            harv.LastHarvestedCell = self.Location;

            if (harv.IsFull)
            {
                claimLayer.RemoveClaim(self);
                return NextActivity;

            }

            if(harvInfo.HarvestFacings != 0)
            {
                var current = facing.Facing;
                var desired = body.QuantizeFacing(current, harvInfo.HarvestFacings);
                if (desired != current)
                    return ActivityUtils.SequenceActivities(new Turn(self, desired), this);
            }

            var resource = resLayer.Harvest(self.Location);
            if(resource == null)
            {
                claimLayer.RemoveClaim(self);
                return NextActivity;
            }

            harv.AcceptResource(resource);

            foreach (var t in self.TraitsImplementing<INotifyHarvesterAction>())
                t.Harvested(self, resource);

            return ActivityUtils.SequenceActivities(new Wait(harvInfo.BaleLoadDelay), this);
        }

    }
}
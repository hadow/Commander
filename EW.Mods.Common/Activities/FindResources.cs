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


        public override Activity Tick(Actor self)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Mods.Common.Activities;
using EW.NetWork;
namespace EW.Mods.Common.Traits
{

    public class HarvesterInfo : ITraitInfo,Requires<MobileInfo>
    {
        /// <summary>
        /// Which resources it can harvest.
        /// </summary>
        public readonly HashSet<string> Resources = new HashSet<string>();

        /// <summary>
        /// Percentage of maximum speed when fully loaded.
        /// </summary>
        public readonly int FullyLoadedSpeed = 85;

        /// <summary>
        /// How much resources it can carry.
        /// </summary>
        public readonly int Capacity = 28;

        /// <summary>
        /// Automatically scan for resources when created.
        /// </summary>
        public readonly bool SearchOnCreation = true;

        [VoiceReference]
        public readonly string HarvestVoice = "Action";

        [VoiceReference]
        public readonly string DeliverVoice = "Action";
        public object Create(ActorInitializer init) { return new Harvester(init.Self, this); }
    }
    public class Harvester:IResolveOrder,ISpeedModifier,ISync,INotifyCreated,INotifyIdle,INotifyBlockingMove
    {
        public readonly HarvesterInfo Info;
        readonly Mobile mobile;
        readonly ResourceLayer resLayer;
        readonly ResourceClaimLayer claimLayer;

        Dictionary<ResourceTypeInfo, int> contents = new Dictionary<ResourceTypeInfo, int>();


        public Harvester(Actor self,HarvesterInfo info)
        {
            Info = info;
            mobile = self.Trait<Mobile>();
            resLayer = self.World.WorldActor.Trait<ResourceLayer>();
            claimLayer = self.World.WorldActor.Trait<ResourceClaimLayer>();


        }

        void INotifyCreated.Created(Actor self)
        {

            if (Info.SearchOnCreation)
                self.World.AddFrameEndTask(w => self.QueueActivity(new FindResources(self)));
        }

        void INotifyBlockingMove.OnNotifyBlockingMove(Actor self, Actor blocking)
        {

        }

        void INotifyIdle.TickIdle(Actor self)
        {

        }

        int ISpeedModifier.GetSpeedModifier()
        {
            return 100 - (100 - Info.FullyLoadedSpeed) * contents.Values.Sum() / Info.Capacity;

        }

        public void ResolveOrder(Actor self,Order order)
        {

        }


        public bool CanHarvestCell(Actor self,CPos cell)
        {
            if (cell.Layer != 0)
                return false;

            var resType = resLayer.GetResource(cell);
            if (resType == null)
                return false;

            //Can the harvester collect this kind of resource?
            return Info.Resources.Contains(resType.Info.Type);
        }
        
    }
}
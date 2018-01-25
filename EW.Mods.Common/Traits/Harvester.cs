using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using EW.Traits;
using EW.Mods.Common.Activities;
using EW.Mods.Common.Pathfinder;
using EW.NetWork;
using EW.Activities;
namespace EW.Mods.Common.Traits
{

    public class HarvesterInfo : ITraitInfo,Requires<MobileInfo>
    {
        /// <summary>
        /// How long (in ticks) to wait until (re-)checking for a nearby available DeliveryBuilding if not yet linked to one.
        /// </summary>

        public readonly int SearchForDeliveryBuildingDelay = 125;

        public readonly HashSet<string> DeliveryBuildings = new HashSet<string>();


        public readonly CVec UnblockCell = new CVec(0, 4);

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

        public readonly int BaleLoadDelay = 4;

        /// <summary>
        /// Automatically scan for resources when created.
        /// </summary>
        public readonly bool SearchOnCreation = true;

        public readonly int HarvestFacings = 0;

        /// <summary>
        /// Maximum duration of being idle before queueing a wait activity.
        /// </summary>
        public readonly int MaxIdleDuration = 25;


        /// <summary>
        /// Duration to wait before becoming idle again.
        /// </summary>
        public readonly int WaitDuration = 25;

        /// <summary>
        /// Find a new refinery to unload at if more than this many harvesters are already waiting.
        /// </summary>
        public readonly int MaxUnloadQueue = 3;

        /// <summary>
        /// The pathfinding cost penalty applied for each harvester waiting to unload at a refinery.
        /// </summary>
        public readonly int UnloadQueueCostModifier = 12;
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

        bool idleSmart = true;
        int idleDuration;

        [Sync]
        public bool LastSearchFailed;

        [Sync]
        public Actor OwnerLinkedProc = null;

        [Sync]
        public Actor LastLinkedProc = null;

        [Sync]
        public Actor LinkedProc = null;

        public CPos? LastHarvestedCell = null;
        public CPos? LastOrderLocation = null;
        public Harvester(Actor self,HarvesterInfo info)
        {
            Info = info;
            mobile = self.Trait<Mobile>();
            resLayer = self.World.WorldActor.Trait<ResourceLayer>();
            claimLayer = self.World.WorldActor.Trait<ResourceClaimLayer>();

            self.QueueActivity(new CallFunc(() => ChooseNewProc(self, null)));

        }

        public bool IsFull { get { return contents.Values.Sum() == Info.Capacity; } }

        public bool IsEmpty { get { return contents.Values.Sum() == 0; } }

        public int Fullness { get { return contents.Values.Sum() * 100 / Info.Capacity; } }
        void INotifyCreated.Created(Actor self)
        {

            if (Info.SearchOnCreation)
                self.World.AddFrameEndTask(w => self.QueueActivity(new FindResources(self)));
        }

        public void AcceptResource(ResourceType type)
        {
            if (!contents.ContainsKey(type.Info))
                contents[type.Info] = 1;
            else
                contents[type.Info]++;
        }

        void INotifyBlockingMove.OnNotifyBlockingMove(Actor self, Actor blocking)
        {
            var act = self.CurrentActivity;

            if(act is Wait)
            {
                self.CancelActivity();

                var cell = self.Location;
                var moveTo = mobile.NearestMoveableCell(cell, 2, 5);
                self.QueueActivity(mobile.MoveTo(moveTo, 0));
                self.SetTargetLine(Target.FromCell(self.World, moveTo), Color.Gray, false);

                //Find more resources but not at this location
                self.QueueActivity(new FindResources(self, cell));
            }
        }

        void INotifyIdle.TickIdle(Actor self)
        {
            if (!idleSmart)
                return;

            if (!IsEmpty)
            {
                self.QueueActivity(new DeliverResources(self));
                return;
            }

            UnblockRefinery(self);

            idleDuration++;

            if(idleDuration > Info.MaxIdleDuration)
            {
                idleDuration = 0;

                self.QueueActivity(new Wait(Info.WaitDuration));
            }
        }


        public void UnblockRefinery(Actor self)
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


        public void ChooseNewProc(Actor self,Actor ignore)
        {
            LastLinkedProc = null;
            LinkProc(self, ClosestProc(self, ignore));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="proc"></param>
        public void LinkProc(Actor self,Actor proc)
        {
            var oldProc = LinkedProc;
            LinkedProc = proc;
            SetProcLines(oldProc);
            SetProcLines(proc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proc"></param>
        public void SetProcLines(Actor proc)
        {
            if (proc == null || proc.IsDead)
                return;

            var linkedHarvs = proc.World.ActorsHavingTrait<Harvester>(h => h.LinkedProc == proc)
                .Select(a => Target.FromActor(a))
                .ToList();

            proc.SetTargetLines(linkedHarvs, Color.Gold);
        }

        public Actor ClosestProc(Actor self,Actor ignore)
        {
            //Find all refineries and their occupancy count;
            var refs = self.World.ActorsWithTrait<IAcceptResources>()
                .Where(r => r.Actor != ignore && r.Actor.Owner == self.Owner && IsAcceptableProcType(r.Actor))
                .Select(r => new
                {
                    Location = r.Actor.Location + r.Trait.DeliveryOffset,
                    Actor = r.Actor,
                    Occupancy = self.World.ActorsHavingTrait<Harvester>(h => h.LinkedProc == r.Actor).Count()
                }).ToDictionary(r => r.Location);

            //Start a search from each refinery's delivery location:
            List<CPos> path;
            var mi = self.Info.TraitInfo<MobileInfo>();

            using (var search = PathSearch.FromPoints(self.World, mi, self, refs.Values.Select(r => r.Location), self.Location, false)
                .WithCustomCost(loc =>
                {
                    if (!refs.ContainsKey(loc))
                        return 0;

                    var occupancy = refs[loc].Occupancy;

                    //Too many harvesters clogs up the refinery's delivery location.
                    if (occupancy >= Info.MaxUnloadQueue)
                        return Constants.InvalidNode;

                    //Prefer refineries with less occupancy (multiplier is to offset distance cost);
                    return occupancy * Info.UnloadQueueCostModifier;

                }))
                path = self.World.WorldActor.Trait<IPathFinder>().FindPath(search);

            if (path.Count != 0)
                return refs[path.Last()].Actor;

            return null;
        }
        

        bool IsAcceptableProcType(Actor proc)
        {
            return Info.DeliveryBuildings.Count == 0 || Info.DeliveryBuildings.Contains(proc.Info.Name);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.NetWork;
namespace EW.Mods.Common.Traits
{

    public class ProductionQueueInfo : ITraitInfo
    {
        /// <summary>
        /// What kind of production will be added (e.g. Building,Infantry,Vehicle)
        /// </summary>
        [FieldLoader.Require]
        public readonly string Type = null;

        public readonly string Group = null;

        /// <summary>
        /// Only enable this queue for certain factions.
        /// </summary>
        public readonly HashSet<string> Factions = new HashSet<string>();

        [Desc("Should the prerequisite remain enabled if the owner changes?")]
        public readonly bool Sticky = true;


        public readonly int BuildDurationModifier = 100;


        public readonly string ReadyAudio = "UnitReady";

        public readonly string BlockedAudio = "NoBuild";

        public readonly string QueuedAudio = "Training";

        public readonly string OnHoldAudio = "OnHold";

        public virtual object Create(ActorInitializer init) { return new ProductionQueue(init,init.Self,this); } 
    }



    public class ProductionQueue:IResolveOrder,ITick,ITechTreeElement,INotifyOwnerChanged,INotifyKilled,INotifySold,ISync,INotifyCreated
    {

        public readonly ProductionQueueInfo Info;

        readonly Actor self;

        readonly Dictionary<ActorInfo, ProductionState> producible = new Dictionary<ActorInfo, ProductionState>();
        readonly List<ProductionItem> queue = new List<ProductionItem>();
        readonly IEnumerable<ActorInfo> allProducibles;
        readonly IEnumerable<ActorInfo> buildableProducibles;

        Production[] productionTraits;

        //Will change if the owner changes
        PowerManager playerPower;
        PlayerResources playerResources;
        protected DeveloperMode developerMode;

        public Actor Actor { get { return self; } }

        [Sync]public int QueueLength { get { return queue.Count; } }

        [Sync] public int CurrentRemainingCost { get { return QueueLength == 0 ? 0 : queue[0].RemainingCost; } }

        [Sync] public int CurrentRemainingTime { get { return QueueLength == 0 ? 0 : queue[0].RemainingTime; } }

        [Sync] public bool Enabled { get; protected set; }
        public string Faction { get; private set; }
        
        [Sync] public bool IsValidFaction { get; private set; }

        public ProductionQueue(ActorInitializer init,Actor playerActor,ProductionQueueInfo info)
        {
            self = init.Self;
            Info = info;
            playerResources = playerActor.Trait<PlayerResources>();
            playerPower = playerActor.Trait<PowerManager>();
            developerMode = playerActor.Trait<DeveloperMode>();

            Faction = init.Contains<FactionInit>() ? init.Get<FactionInit, string>() : self.Owner.Faction.InternalName;

            IsValidFaction = !info.Factions.Any() || info.Factions.Contains(Faction);

        }

        void INotifyCreated.Created(Actor self)
        {
            productionTraits = self.TraitsImplementing<Production>().ToArray();
        }

        void INotifyOwnerChanged.OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
        {
            ClearQueue();

            playerPower = newOwner.PlayerActor.Trait<PowerManager>();
            playerResources = newOwner.PlayerActor.Trait<PlayerResources>();
            developerMode = newOwner.PlayerActor.Trait<DeveloperMode>();


        }

        public virtual TraitPair<Production> MostLikelyProducer()
        {
            var traits = productionTraits.Where(p => !p.IsTraitDisabled && p.Info.Produces.Contains(Info.Type));
            var unpaused = traits.FirstOrDefault(a => !a.IsTraitPaused);
            return new TraitPair<Production>(self, unpaused != null ? unpaused : traits.FirstOrDefault());
        }

        void INotifyKilled.Killed(Actor killed, AttackInfo attackInfo)
        {
            if(killed == self)
            {

            }
        }


        void INotifySold.Selling(Actor self)
        {

        }

        void INotifySold.Sold(Actor self)
        {

        }

        void ITick.Tick(Actor self)
        {
            Tick(self);
        }


        protected virtual void Tick(Actor self)
        {

        }


        protected virtual void TickInner(Actor self,bool allProductioniPaused)
        {

        }

        public virtual IEnumerable<ActorInfo> AllItems()
        {
            return allProducibles;

        }

        public virtual IEnumerable<ActorInfo> BuildableItems()
        {

            if (!Enabled)
                return Enumerable.Empty<ActorInfo>();

            return buildableProducibles;
        }


        public bool CanBuild(ActorInfo ai)
        {
            ProductionState ps;

            if (!producible.TryGetValue(ai, out ps))
                return false;

            return ps.Buildable;
        }

        protected void BeginProduction(ProductionItem item)
        {
            queue.Add(item);
        }

        public void FinishProduction()
        {
            if (queue.Count != 0)
                queue.RemoveAt(0);
        }

        protected void ClearQueue()
        {
            if (queue.Count == 0)
                return;

            playerResources.GiveCash(queue[0].TotalCost - queue[0].RemainingCost);
            queue.Clear();
        }

        
        protected virtual bool BuildUnit(ActorInfo unit)
        {
            return false;
        }

        protected void CancelProduction(string itemName,uint numberToCancel)
        {

        }

        void CancelProductionInner(string itemName)
        {

        }


        public virtual int GetBuildTime(ActorInfo unit,BuildableInfo bi)
        {

            var time = bi.BuildDuration;

            if(time == -1)
            {
                var valued = unit.TraitInfoOrDefault<ValuedInfo>();
                time = valued != null ? valued.Cost : 0;
            }

            time = time * bi.BuildDurationModifier * Info.BuildDurationModifier / 10000;
            return time;
        }
        public void PrerequisitesAvailable(string key)
        {

        }

        public void PrerequisitesUnavailable(string key)
        {

        }

        public void PrerequisitesItemHidden(string key)
        {

        }


        public void PrerequisitesItemVisible(string key)
        {

        }

        public void ResolveOrder(Actor self,Order order)
        {
            if (!Enabled)
                return;

            var rules = self.World.Map.Rules;

            switch (order.OrderString)
            {
                case "StartProduction":
                    var unit = rules.Actors[order.TargetString];
                    var bi = unit.TraitInfo<BuildableInfo>();

                    if (!bi.Queue.Contains(Info.Type))
                        return;

                    if (BuildableItems().All(b => b.Name != order.TargetString))
                        return;

                    var fromLimit = int.MaxValue;
                    if(bi.BuildLimit > 0)
                    {
                        var inQueue = queue.Count(pi => pi.Item == order.TargetString);
                        var owned = self.Owner.World.ActorsHavingTrait<Buildable>().Count(a => a.Info.Name == order.TargetString && a.Owner == self.Owner);
                        fromLimit = bi.BuildLimit - (inQueue + owned);

                        if (fromLimit <= 0)
                            return;
                    }

                    var valued = unit.TraitInfoOrDefault<ValuedInfo>();
                    var cost = valued != null ? valued.Cost : 0;
                    var time = GetBuildTime(unit, bi);
                    var amountToBuild = Math.Min(fromLimit, order.ExtraData);

                    for(var n = 0; n < amountToBuild; n++)
                    {
                        var hasPlayedSound = false;
                        BeginProduction(new ProductionItem(this, order.TargetString, cost, playerPower, () => self.World.AddFrameEndTask(_ =>
                        {
                            var isBuilding = unit.HasTraitInfo<BuildingInfo>();

                            if (isBuilding && !hasPlayedSound)
                                hasPlayedSound = WarGame.Sound.PlayNotification(rules, self.Owner, "Speech", Info.ReadyAudio, self.Owner.Faction.InternalName);
                            else if (!isBuilding)
                            {
                                if (BuildUnit(unit))
                                    WarGame.Sound.PlayNotification(rules, self.Owner, "Speech", Info.ReadyAudio, self.Owner.Faction.InternalName);
                                else if (!hasPlayedSound && time > 0)
                                    hasPlayedSound = WarGame.Sound.PlayNotification(rules, self.Owner, "Speech", Info.BlockedAudio, self.Owner.Faction.InternalName);
                            }
                        })));
                    }
                    break;
            }
        }

        public ProductionItem CurrentItem()
        {
            return queue.ElementAtOrDefault(0);
        }
    }


    public class ProductionState
    {
        public bool Visible = true;
        public bool Buildable = false;
    }

    public class ProductionItem
    {
        public readonly string Item;
        public readonly ProductionQueue Queue;
        public readonly int TotalCost;
        public readonly Action OnComplete;



        public int TotalTime { get; private set; }

        public int RemainingTime { get; private set; }

        public int RemainingCost { get; private set; }

        public bool Paused { get; private set; }

        public bool Done { get; private set; }

        public bool Started { get; private set; }

        public int Slowdown { get; private set; }

        readonly ActorInfo ai;
        readonly BuildableInfo bi;
        readonly PowerManager pm;


        public ProductionItem(ProductionQueue queue,string item,int cost,PowerManager pm,Action onComplete)
        {
            Item = item;
            RemainingTime = TotalTime = 1;
            RemainingCost = TotalCost = cost;
            OnComplete = onComplete;
            Queue = queue;
            this.pm = pm;
            ai = Queue.Actor.World.Map.Rules.Actors[item];
            bi = ai.TraitInfo<BuildableInfo>();
        }


        public void Tick(PlayerResources pr)
        {


        }
    }
}
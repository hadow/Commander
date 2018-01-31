using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Mods.Common.Activities;
using EW.Activities;
using EW.Mods.Common.Effects;
namespace EW.Mods.Common.Traits
{

    /// <summary>
    /// 精练厂
    /// </summary>
    public class RefineryInfo : Requires<WithSpriteBodyInfo>,IAcceptResourcesInfo
    {
        public readonly int DockAngle = 0;

        public readonly CVec DockOffset = CVec.Zero;

        public readonly bool IsDragRequired = false;

        public readonly WVec DragOffset = WVec.Zero;

        public readonly int DragLength = 0;

        public readonly bool UseStorage = true;

        /// <summary>
        /// Discard resources once silo capacity has been reached.
        /// </summary>
        public readonly bool DiscardExcessResources = false;


        public readonly bool ShowTicks = true;
        public readonly int TickLifetime = 30;
        public readonly int TickVelocity = 2;
        public readonly int TickRate = 10;


        public virtual object Create(ActorInitializer init) { return new Refinery(init.Self,this); }
    }
    public class Refinery:ITick,IAcceptResources,INotifyOwnerChanged,ISync,INotifyActorDisposing,IExplodeModifier
    {

        readonly Actor self;
        readonly RefineryInfo info;
        PlayerResources playerResources;

        int currentDisplayTick = 0;
        int currentDisplayValue = 0;

        [Sync]
        bool preventDock = false;
        [Sync]public int Ore = 0;
        [Sync]Actor dockedHarv = null;

        public bool AllowDocking { get { return !preventDock; } }

        public CVec DeliveryOffset { get { return info.DockOffset; } }

        public int DeliveryAngle { get { return info.DockAngle; } }

        public bool IsDragRequired { get { return info.IsDragRequired; } }

        public WVec DragOffset { get { return info.DragOffset; } }

        public int DragLength { get { return info.DragLength; } }



        public Refinery(Actor self,RefineryInfo info)
        {
            this.self = self;
            this.info = info;
            playerResources = self.Owner.PlayerActor.Trait<PlayerResources>();
            currentDisplayTick = info.TickRate;
        }

        public bool CanGiveResource(int amount)
        {
            return !info.UseStorage || info.DiscardExcessResources || playerResources.CanGiveResources(amount);
        }

        public void GiveResource(int amount)
        {
            if (info.UseStorage)
            {
                if (info.DiscardExcessResources)
                    amount = Math.Min(amount, playerResources.ResourceCapacity - playerResources.Resources);

                playerResources.GiveResources(amount);
            }
            else
                playerResources.GiveCash(amount);

            if (info.ShowTicks)
                currentDisplayValue += amount;
        }

        public void OnDock(Actor harv,DeliverResources dockOrder)
        {
            if (!preventDock)
            {
                dockOrder.Queue(new CallFunc(() => dockedHarv = harv, false));
                dockOrder.Queue(DockSequence(harv, self));
                dockOrder.Queue(new CallFunc(() => dockedHarv = null, false));
            }

            dockOrder.Queue(new CallFunc(()=>harv.Trait<Harvester>().ContinueHarvesting(harv)));
        }

        void CancelDock(Actor self)
        {
            preventDock = true;

            //Cancel the dock sequence
            if (dockedHarv != null && !dockedHarv.IsDead)
                dockedHarv.CancelActivity();
        }


        public virtual Activity DockSequence(Actor harv,Actor self)
        {
            return new SpriteHarvesterDockSequence(harv, self, DeliveryAngle, IsDragRequired, DragOffset, DragLength);
        }


        void ITick.Tick(Actor self)
        {
            if (dockedHarv != null && dockedHarv.IsDead)
                dockedHarv = null;

            if(info.ShowTicks && currentDisplayValue > 0 && --currentDisplayTick <= 0)
            {
                var temp = currentDisplayValue;
                if(self.Owner.IsAlliedWith(self.World.RenderPlayer))
                {
                    self.World.AddFrameEndTask(w => w.Add(new FloatingText(self.CenterPosition, self.Owner.Color.RGB, FloatingText.FormatCashTick(temp), 30)));
                }
                currentDisplayTick = info.TickRate;
                currentDisplayValue = 0;
            }
        }


        public IEnumerable<TraitPair<Harvester>> GetLinkedHarvesters()
        {
            return self.World.ActorsWithTrait<Harvester>().Where(a => a.Trait.LinkedProc == self);
        }

        void INotifyOwnerChanged.OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
        {
            foreach (var harv in GetLinkedHarvesters())
                harv.Trait.UnlinkProc(harv.Actor, self);

            playerResources = newOwner.PlayerActor.Trait<PlayerResources>();

        }

        void INotifyActorDisposing.Disposing(Actor self)
        {
            CancelDock(self);
            foreach (var harv in GetLinkedHarvesters())
                harv.Trait.UnlinkProc(harv.Actor, self);
        }

        public bool ShouldExplode(Actor self) { return Ore > 0; }


    }


}
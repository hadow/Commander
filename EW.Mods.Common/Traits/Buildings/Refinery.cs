using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Mods.Common.Activities;
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

        public readonly bool DiscardExcessResources = false;


        public readonly bool ShowTicks = true;
        public readonly int TickLifetime = 30;
        public readonly int TickVelocity = 2;
        public readonly int TickRate = 10;


        public virtual object Create(ActorInitializer init) { return new Refinery(init.Self,this); }
    }
    public class Refinery:ITick,IAcceptResources,INotifyOwnerChanged,ISync,INotifyActorDisposing
    {

        readonly Actor self;
        readonly RefineryInfo info;
        PlayerResources playerResources;

        int currentDisplayTick = 0;
        int currentDisplayValue = 0;

        [Sync]
        bool preventDock = false;

        public bool AllowDocking { get { return !preventDock; } }

        public CVec DeliveryOffset { get { return info.DockOffset; } }

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

        }

        public void OnDock(Actor harv,DeliverResources dockOrder)
        {

        }


        void ITick.Tick(Actor self)
        {

        }

        void INotifyOwnerChanged.OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
        {

        }

        void INotifyActorDisposing.Disposing(Actor self)
        {

        }


    }


}
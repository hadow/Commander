using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{


    public class PowerInfo : ConditionalTraitInfo
    {
        [Desc("If negative, it will drain power. If positive, it will provide power.")]
        public readonly int Amount = 0;

        public override object Create(ActorInitializer init)
        {
            return new Power(init.Self, this);
        }
    }
    public class Power:ConditionalTrait<PowerInfo>,INotifyAddedToWorld, INotifyRemovedFromWorld, INotifyOwnerChanged
    {

        public PowerManager PlayerPower { get; private set; }


        public Power(Actor self,PowerInfo info) : base(info)
        {
            PlayerPower = self.Owner.PlayerActor.Trait<PowerManager>();

        }


        protected override void TraitEnabled(Actor self) { PlayerPower.UpdateActor(self); }
        protected override void TraitDisabled(Actor self) { PlayerPower.UpdateActor(self); }

        void INotifyAddedToWorld.AddedToWorld(Actor self) { PlayerPower.UpdateActor(self); }
        void INotifyRemovedFromWorld.RemovedFromWorld(Actor self) { PlayerPower.RemoveActor(self); }

        void INotifyOwnerChanged.OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
        {
            PlayerPower.RemoveActor(self);
            PlayerPower = newOwner.PlayerActor.Trait<PowerManager>();
            PlayerPower.UpdateActor(self);
        }


    }
}
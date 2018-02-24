using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    class ActorLostNotificationInfo : ITraitInfo
    {
        public readonly string Notification = "UnitLost";

        public readonly bool NotifyAll = false;

        public object Create(ActorInitializer init)
        {
            return new ActorLostNotification(this);
        }
    }
    class ActorLostNotification:INotifyKilled
    {

        ActorLostNotificationInfo info;

        public ActorLostNotification(ActorLostNotificationInfo info)
        {
            this.info = info;
        }

        public void Killed(Actor self,AttackInfo ai)
        {
            var player = info.NotifyAll ? self.World.LocalPlayer : self.Owner;

            WarGame.Sound.PlayNotification(self.World.Map.Rules, player, "Speech", info.Notification, self.Owner.Faction.InternalName);
        }


    }
}
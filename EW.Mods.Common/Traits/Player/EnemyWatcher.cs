using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    class EnemyWatcherInfo:ITraitInfo
    {
        public readonly int ScanInterval = 25;

        public readonly int NotificationInterval = 750;

        public object Create(ActorInitializer init) { return new EnemyWatcher(this); }
    }

    class EnemyWatcher : ITick
    {
        readonly EnemyWatcherInfo info;

        readonly HashSet<Player> discoveredPlayers;

        HashSet<uint> lastKnownActorIds;
        HashSet<uint> visibleActorIds;
        HashSet<string> playedNotifications;

        bool announcedAny;
        int rescanInterval;
        int ticksBeforeNextNotification;
        public EnemyWatcher(EnemyWatcherInfo info)
        {
            this.info = info;
            discoveredPlayers = new HashSet<Player>();
            lastKnownActorIds = new HashSet<uint>();
            rescanInterval = 0;
            ticksBeforeNextNotification = 0;
        }
        public void Tick(Actor self)
        {
            if (self.Owner.Shroud.Disabled || self.Owner.IsBot || !self.Owner.Playable || self.Owner.PlayerReference.Spectating)
                return;

            rescanInterval--;
            ticksBeforeNextNotification--;

            if (rescanInterval > 0)
                return;

            rescanInterval = info.ScanInterval;

            announcedAny = false;
            visibleActorIds = new HashSet<uint>();
            playedNotifications = new HashSet<string>();

            foreach(var announceSeen in self.World.ActorsWithTrait<AnnounceOnSeen>())
            {
                if (announceSeen.Actor.AppearsFriendlyTo(self))
                {
                    continue;
                }

                if (announceSeen.Actor.IsDead || !announceSeen.Actor.IsInWorld)
                    continue;

                if (!self.Owner.CanViewActor(announceSeen.Actor))
                    continue;

                visibleActorIds.Add(announceSeen.Actor.ActorID);

                if (lastKnownActorIds.Contains(announceSeen.Actor.ActorID))
                    continue;

                var playNotification = !playedNotifications.Contains(announceSeen.Trait.Info.Notification) && ticksBeforeNextNotification <= 0;

                foreach (var trait in announceSeen.Actor.TraitsImplementing<INotifyDiscovered>())
                    trait.OnDiscovered(announceSeen.Actor, self.Owner, playNotification);

                var discoveredPlayer = announceSeen.Actor.Owner;

                if (!discoveredPlayers.Contains(discoveredPlayer))
                {
                    foreach (var trait in discoveredPlayer.PlayerActor.TraitsImplementing<INotifyDiscovered>())
                        trait.OnDiscovered(announceSeen.Actor, self.Owner, false);

                    discoveredPlayers.Add(discoveredPlayer);
                }

                if (!playNotification)
                    continue;

                playedNotifications.Add(announceSeen.Trait.Info.Notification);
                announcedAny = true;
            }

            if (announcedAny)
                ticksBeforeNextNotification = info.NotificationInterval;

            lastKnownActorIds = visibleActorIds;

        }
            
    }
}
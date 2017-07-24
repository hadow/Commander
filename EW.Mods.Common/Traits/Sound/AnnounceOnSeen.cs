using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class AnnounceOnSeenInfo:ITraitInfo
    {
        public readonly bool PingRadar = false;

        public readonly string Notification = null;

        public readonly bool AnnounceNeutrals = false;

        public object Create(ActorInitializer init) { return new AnnounceOnSeen(init.Self, this); }
    }

    public class AnnounceOnSeen : INotifyDiscovered
    {
        public readonly AnnounceOnSeenInfo Info;

        readonly Lazy<RadarPings> radarPings;
        public AnnounceOnSeen(Actor self,AnnounceOnSeenInfo info)
        {
            Info = info;
            radarPings = Exts.Lazy(() => self.World.WorldActor.Trait<RadarPings>());
        }

        public void OnDiscovered(Actor self,Player discoverer,bool playNotification)
        {

        }

    }
}
using System;
using EW.Traits;
using EW.NetWork;
namespace EW.Mods.Common.Traits
{

    public class PlaceSimpleBeaconInfo : ITraitInfo
    {
        public readonly int Duration = 30 * 25;

        public readonly string NotificationType = "Sounds";

        public readonly string Notification = "Beacon";

        public readonly bool IsPlayerPalette = false;

        [PaletteReference("IsPlayerPalette")]
        public readonly string Palette = "effect";

        [SequenceReference("BeaconImage")]
        public readonly string BeaconSequence = "idle";


        public object Create(ActorInitializer init)
        {
            return new PlaceSimpleBeacon(init.Self,this);
        }
    }

    public class PlaceSimpleBeacon:IResolveOrder
    {
        readonly PlaceSimpleBeaconInfo info;
        readonly RadarPings radarPings;

        RadarPing playerRadarPing;
        
        public PlaceSimpleBeacon(Actor self,PlaceSimpleBeaconInfo info)
        {
            radarPings = self.World.WorldActor.TraitOrDefault<RadarPings>();
            this.info = info;
        }

        void IResolveOrder.ResolveOrder(Actor self, Order order)
        {

        }
    }
}
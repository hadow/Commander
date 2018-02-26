using System;
using EW.Traits;
using EW.NetWork;
using EW.Mods.Cnc.Effects;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.Traits
{

    [Desc("A beacon that consists of a single sprite that can be animated.")]
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

        public readonly string BeaconImage = "beacon";

        public object Create(ActorInitializer init)
        {
            return new PlaceSimpleBeacon(init.Self,this);
        }
    }

    public class PlaceSimpleBeacon:IResolveOrder
    {
        readonly PlaceSimpleBeaconInfo info;
        readonly RadarPings radarPings;

        AnimatedBeacon playerBeacon;

        RadarPing playerRadarPing;
        
        public PlaceSimpleBeacon(Actor self,PlaceSimpleBeaconInfo info)
        {
            radarPings = self.World.WorldActor.TraitOrDefault<RadarPings>();
            this.info = info;
        }

        void IResolveOrder.ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString != "PlaceBeacon")
                return;

            var pos = self.World.Map.CenterOfCell(order.TargetLocation);

            self.World.AddFrameEndTask(w =>
            {
                if (playerBeacon != null)
                    self.World.Remove(playerBeacon);

                playerBeacon = new AnimatedBeacon(self.Owner, pos, info.Duration, info.Palette, info.IsPlayerPalette, info.BeaconImage, info.BeaconSequence);
                self.World.Add(playerBeacon);

                if (self.Owner.IsAlliedWith(self.World.RenderPlayer))
                    WarGame.Sound.PlayNotification(self.World.Map.Rules, null, info.NotificationType, info.Notification, self.World.RenderPlayer != null ? self.World.RenderPlayer.Faction.InternalName : null);

                if(radarPings != null)
                {
                    if (playerRadarPing != null)
                        radarPings.Remove(playerRadarPing);

                    playerRadarPing = radarPings.Add(() => self.Owner.IsAlliedWith(self.World.RenderPlayer), pos, self.Owner.Color.RGB, info.Duration);
                }
            });
        }
    }
}
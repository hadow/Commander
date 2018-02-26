using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class PlaceBuildingInfo : ITraitInfo
    {

        [Desc("Palette to use for rendering the placement sprite.")]
        [PaletteReference]public readonly string Palette = TileSet.TerrainPaletteInternalName;


        [Desc("Palette to use for rendering the placement sprite for line build segments.")]
        [PaletteReference]public readonly string LineBuildSegmentPalette = TileSet.TerrainPaletteInternalName;

        public readonly int NewOptionsNotificationDelay = 10;

        public readonly string NewOptionsNotification = "NewOptions";
        
        public object Create(ActorInitializer init) { return new PlaceBuilding(this); }
    }
    public class PlaceBuilding:IResolveOrder,ITick
    {
        readonly PlaceBuildingInfo info;
        bool triggerNotification;
        int tick;

        public PlaceBuilding(PlaceBuildingInfo info)
        {
            this.info = info;
        }


        void IResolveOrder.ResolveOrder(Actor self, NetWork.Order order)
        {

        }

        void ITick.Tick(Actor self)
        {
            if (!triggerNotification)
                return;

            if (tick++ >= info.NewOptionsNotificationDelay)
                PlayNotification(self);
        }


        void PlayNotification(Actor self)
        {
            WarGame.Sound.PlayNotification(self.World.Map.Rules, self.Owner, "Speech", info.NewOptionsNotification, self.Owner.Faction.InternalName);
            triggerNotification = false;
            tick = 0;
        }
    }
}
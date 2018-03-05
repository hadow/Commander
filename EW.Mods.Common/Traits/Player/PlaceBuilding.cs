﻿using System;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    [Desc("Allows the player to execute build orders.", " Attach this to the player actor.")]
    public class PlaceBuildingInfo : ITraitInfo
    {

        [Desc("Palette to use for rendering the placement sprite.")]
        [PaletteReference]public readonly string Palette = TileSet.TerrainPaletteInternalName;


        [Desc("Palette to use for rendering the placement sprite for line build segments.")]
        [PaletteReference]public readonly string LineBuildSegmentPalette = TileSet.TerrainPaletteInternalName;

        [Desc("Play NewOptionsNotification this many ticks after building placement.")]
        public readonly int NewOptionsNotificationDelay = 10;

        [Desc("Notification to play after building placement if new construction options are available.")]
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

            var os = order.OrderString;
            if (os != "PlaceBuilding" &&
                os != "LineBuild" &&
                os != "PlacePlug")
                return;

            self.World.AddFrameEndTask(w=>{



                var targetActor = w.GetActorById(order.ExtraData);

                if (targetActor == null || targetActor.IsDead)
                    return;

                var unit = self.World.Map.Rules.Actors[order.TargetString];
                var queue = targetActor.TraitsImplementing<ProductionQueue>()
                                       .FirstOrDefault(q => q.CanBuild(unit) && q.CurrentItem() != null && q.CurrentItem().Item == order.TargetString && q.CurrentItem().RemainingTime == 0);

                if (queue == null)
                    return;

                var producer = queue.MostLikelyProducer();
                var faction = producer.Trait != null ? producer.Trait.Faction : self.Owner.Faction.InternalName;

                var buildingInfo = unit.TraitInfo<BuildingInfo>();
                var buildableInfo = unit.TraitInfoOrDefault<BuildableInfo>();

                if(buildableInfo != null && buildableInfo.ForceFaction != null ){

                    faction = buildableInfo.ForceFaction;
                }

                if(os == "LineBuild"){


                }
                else if(os == "PlacePlug"){


                }
                else{


                }


            });

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
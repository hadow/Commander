using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Primitives;
namespace EW.Mods.Common.Traits
{

    public class CrateSpawnerInfo : ITraitInfo
    {
        [Desc("Average time (ticks) between crate spawn.")]
        public readonly int SpawnInterval = 180 * 25;

        [Desc("Delay (in ticks) before the first crate spawns.")]
        public readonly int InitialSpawnDelay = 0;

        [Desc("Which terrain types can we drop on?")]
        public readonly HashSet<string> ValidGround = new HashSet<string>() { "Clear", "Rough", "Road", "Ore", "Beach" };

        [Desc("Which terrain types count as water?")]
        public readonly HashSet<string> ValidWater = new HashSet<string>() { "Water" };

        [Desc("Chance of generating a water crate instead of a land crate.")]
        public readonly int WaterChance = 20;

        [Desc("Crate actors to drop.")]
        [ActorReference]
        public readonly string[] CrateActors = { "crate" };

        [Desc("Chance of each crate actor spawning.")]
        public readonly int[] CrateActorShares = { 10 };

        [Desc("Number of facings that the delivery aircraft may approach from.")]
        public readonly int QuantizedFacings = 32;


        [Desc("If a DeliveryAircraft: is specified, then this actor will deliver crates.")]
        [ActorReference]
        public readonly string DeliveryAircraft = null;


        public readonly WDist Cordon = new WDist(5120);

        [Desc("Default value of the crates checkbox in the lobby.")]
        public readonly bool CheckboxEnabled = true;


        [Desc("Minimum number of crates.")]
        public readonly int Minimum = 1;

        [Desc("Maximum number of crates.")]
        public readonly int Maximum = 255;

        public object Create(ActorInitializer init) { return new CrateSpawner(init.Self,this); }
    }


    public class CrateSpawner:ITick,INotifyCreated
    {

        readonly Actor self;
        readonly CrateSpawnerInfo info;
        bool enabled;
        int crates;
        int ticks;

        public CrateSpawner(Actor self,CrateSpawnerInfo info){

            this.self = self;
            this.info = info;

            ticks = info.InitialSpawnDelay;


        }
        void INotifyCreated.Created(Actor self){


            enabled = self.World.LobbyInfo.GlobalSettings
                .OptionOrDefault("crates", info.CheckboxEnabled);

        }


        void ITick.Tick(Actor self){

            if (!enabled)
                return;

            if (--ticks <= 0)
            {
                ticks = info.SpawnInterval;

                var toSpawn = Math.Max(0, info.Minimum - crates)
                    + (crates < info.Maximum ? 1 : 0);

                for (var n = 0; n < toSpawn; n++)
                    SpawnCrate(self);
            }
        }


        void SpawnCrate(Actor self){

            var inWater = self.World.SharedRandom.Next(100) < info.WaterChance;
            var pp = ChooseDropCell(self, inWater, 100);

            if (pp == null)
                return;

            var p = pp.Value;
            var crateActor = ChooseCrateActor();

            self.World.AddFrameEndTask(w=>{


                if (info.DeliveryAircraft != null)
                {


                }
                else
                    w.CreateActor(crateActor, new TypeDictionary() { new OwnerInit(w.WorldActor.Owner), new LocationInit(p) });
            });
        }

        CPos? ChooseDropCell(Actor self,bool inWater,int maxTries){


            for (var n = 0; n < maxTries;n++){


                var p = self.World.Map.ChooseRandomCell(self.World.SharedRandom);

                // Is this valid terrain?
                var terrainType = self.World.Map.GetTerrainInfo(p).Type;
                if (!(inWater ? info.ValidWater : info.ValidGround).Contains(terrainType))
                    continue;

                // Don't drop on any actors
                if (self.World.WorldActor.Trait<BuildingInfluence>().GetBuildingAt(p) != null
                    || self.World.ActorMap.GetActorsAt(p).Any())
                    continue;

                return p;
            }

            return null;
        }


        string ChooseCrateActor()
        {
            var crateShares = info.CrateActorShares;
            var n = self.World.SharedRandom.Next(crateShares.Sum());

            var cumulativeShares = 0;
            for (var i = 0; i < crateShares.Length; i++)
            {
                cumulativeShares += crateShares[i];
                if (n <= cumulativeShares)
                    return info.CrateActors[i];
            }

            return null;
        }

        public void IncrementCrates()
        {
            crates++;
        }

        public void DecrementCrates()
        {
            crates--;
        }
    }
}
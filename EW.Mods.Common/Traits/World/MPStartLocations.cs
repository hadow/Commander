using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Graphics;
using EW.Primitives;
namespace EW.Mods.Common.Traits
{

    public class MPStartLocationsInfo : ITraitInfo
    {
        public readonly WDist InitialExploreRang = WDist.FromCells(5);


        public virtual object Create(ActorInitializer init) { return new MPStartLocations(this); }
    }

    public class MPStartLocations:IWorldLoaded
    {
        readonly MPStartLocationsInfo info;

        public readonly Dictionary<Player, CPos> Start = new Dictionary<Player, CPos>();
        public MPStartLocations(MPStartLocationsInfo info)
        {
            this.info = info;
        }

        void IWorldLoaded.WorldLoaded(World world,WorldRenderer wr)
        {
            var spawns = world.Actors.Where(a => a.Info.Name == "mpspawn").Select(a => a.Location).ToArray();

            var taken = world.LobbyInfo.Clients.Where(c => c.SpawnPoint != 0 && c.Slot != null).Select(c => spawns[c.SpawnPoint - 1]).ToList();

            var available = spawns.Except(taken).ToList();

            //Set spawn
            foreach(var kv in world.LobbyInfo.Slots){

                var player = FindPlayerInSlot(world, kv.Key);
                if (player == null)
                    continue;

                var client = world.LobbyInfo.ClientInSlot(kv.Key);

                var spid = (client == null || client.SpawnPoint == 0) ? ChooseSpawnPoint(world, available, taken) : spawns[client.SpawnPoint - 1];

                Start.Add(player,spid);

                player.SpawnPoint = (client == null || client.SpawnPoint == 0) ? spawns.IndexOf(spid) + 1 : client.SpawnPoint;


            }
            //Explore allied shroud
            var map = world.Map;

            foreach(var p in Start.Keys){

                var cells = Shroud.ProjectedCellsInRange(map, Start[p], info.InitialExploreRang);
                foreach(var q in world.Players){

                    if (p.IsAlliedWith(q))
                        q.Shroud.ExploreProjectedCells(world, cells);


                }
            }

            //Set viewport
            if (world.LocalPlayer != null && Start.ContainsKey(world.LocalPlayer))
                wr.ViewPort.Center(map.CenterOfCell(Start[world.LocalPlayer]));

        }


        static Player FindPlayerInSlot(World world,string pr){
            
            return world.Players.FirstOrDefault(p => p.PlayerReference.Name == pr);

        }


        static CPos ChooseSpawnPoint(World world,List<CPos> available,List<CPos> taken){

            if (available.Count == 0)
                throw new InvalidOperationException("No free sapwnpoint");


            var n = taken.Count == 0 ? world.SharedRandom.Next(available.Count) :
                         available.Select((k, i) => Pair.New(k, i)).MaxBy(a => taken.Sum(t => (t - a.First).LengthSquard)).Second;

            var sp = available[n];
            available.RemoveAt(n);
            taken.Add(sp);
            return sp;
        }

    }
}
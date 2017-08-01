using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Graphics;
using System.Linq;
namespace EW.Mods.Common.Traits
{


    public class SpawnMapActorsInfo : TraitInfo<SpawnMapActors>{}



    public class SpawnMapActors:IWorldLoaded
    {
        public Dictionary<string, Actor> Actors = new Dictionary<string, Actor>();

        public uint LastMapActorID { get; private set; }

        public void WorldLoaded(World world,WorldRenderer wr)
        {
            foreach (var kv in world.Map.ActorDefinitions)
            {
                var actorReference = new ActorReference(kv.Value.Value, kv.Value.ToDictionary());

                var ownerName = actorReference.InitDict.Get<OwnerInit>().PlayerName;

                Console.WriteLine("ownername:" + ownerName);
                //If there is no real player associated,don't spawn it.
                if (!world.Players.Any(p => p.InternalName == ownerName))
                    continue;
                var initDict = actorReference.InitDict;
                initDict.Add(new SkipMakeAnimsInit());
                initDict.Add(new SpawnedByMapInit(kv.Key));

                var actor = world.CreateActor(actorReference.Type, initDict);
                Actors[kv.Key] = actor;
                LastMapActorID = actor.ActorID;
            }
        }
    }

    public class SkipMakeAnimsInit : IActorInit, ISuppressInitExport{}

    public class SpawnedByMapInit : IActorInit<string>, ISuppressInitExport
    {
        public readonly string Name;

        public SpawnedByMapInit(string name) { Name = name; }

        public string Value(World world) { return Name; }
    }
}
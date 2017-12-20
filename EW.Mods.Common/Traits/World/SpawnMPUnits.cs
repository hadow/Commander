using System;
using EW.Traits;
using EW.Graphics;
namespace EW.Mods.Common.Traits
{

    public class SpawnMPUnitsInfo : ITraitInfo
    {


        public readonly string StartingUnitsClass = "none";

        public bool Locked = false;
        public object Create(ActorInitializer init) { return new SpawnMPUnits(); }
    }

    public class SpawnMPUnits:IWorldLoaded
    {


        public void WorldLoaded(World world,WorldRenderer wr)
        {
            foreach(var s in world.WorldActor.Trait<MPStartLocations>().Start)
            {

            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Graphics;
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

        public void WorldLoaded(World world,WorldRenderer wr)
        {
            var spawns = world.Actors.Where(a => a.Info.Name == "mpspawn").Select(a => a.Location).ToArray();

        }

    }
}
using System;

using EW.Traits;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// a dictionary of buildings placed on the map.Attach this to the world actor.
    /// </summary>
    public class BuildingInfluenceInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new BuildingInfluence(init.World);
        }
    }
    public class BuildingInfluence
    {
        readonly Map map;
        readonly CellLayer<Actor> influence;
     
        public BuildingInfluence(World world)
        {
            map = world.Map;

            influence = new CellLayer<Actor>(map);

            world.ActorAdded += a =>
            {
                var b = a.Info.TraitInfoOrDefault<BuildingInfo>();
                if (b == null)
                    return;

                foreach(var u in b.Tiles(a.Location))
                {
                    if (influence.Contains(u) && influence[u] == null)
                        influence[u] = a;
                }
            };


            world.ActorRemoved += a =>
            {
                var b = a.Info.TraitInfoOrDefault<BuildingInfo>();
                if (b == null)
                    return;

                foreach(var u in b.Tiles(a.Location))
                {
                    if (influence.Contains(u) && influence[u] == a)
                        influence[u] = null;
                }
            };
        }


        public Actor GetBuildingAt(CPos cell)
        {
            return influence.Contains(cell) ? influence[cell] : null;
        }
    }
}
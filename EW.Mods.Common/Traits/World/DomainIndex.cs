using System;
using System.Linq;
using System.Collections.Generic;
using EW.Graphics;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    /// <summary>
    /// Identify untraversable regions of the map for faster pathfinding,especially with AI.
    /// </summary>
    public class DomainIndexInfo : TraitInfo<DomainIndex> { }


    public class DomainIndex:IWorldLoaded
    {
        Dictionary<uint, MovementClassDomainIndex> domainIndexes;
        public void WorldLoaded(World world,WorldRenderer wr)
        {
            domainIndexes = new Dictionary<uint, MovementClassDomainIndex>();

            var tileSet = world.Map.Rules.TileSet;

            var movementClasses = world.Map.Rules.Actors.Where(ai => ai.Value.HasTraitInfo<MobileInfo>()).Select(ai => (uint)ai.Value.TraitInfo<MobileInfo>().GetMovementClass(tileSet)).Distinct();

            foreach(var mc in movementClasses)
            {
                domainIndexes[mc] = new MovementClassDomainIndex(world, mc);
            }
        }

        public bool IsPassable(CPos p1,CPos p2,uint movementClass)
        {
            return domainIndexes[movementClass].IsPassable(p1, p2); 
        }

    }

    /// <summary>
    /// 
    /// </summary>
    class MovementClassDomainIndex
    {
        readonly Map map;
        readonly uint movementClass;
        readonly CellLayer<int> domains;
        readonly Dictionary<int, HashSet<int>> transientConnections;

        public MovementClassDomainIndex(World world,uint movementClass)
        {
            map = world.Map;
            this.movementClass = movementClass;
            domains = new CellLayer<int>(world.Map);
            transientConnections = new Dictionary<int, HashSet<int>>();
        }

        public bool IsPassable(CPos p1,CPos p2)
        {
            if (!domains.Contains(p1) || !domains.Contains(p2))
                return false;

            if (domains[p1] == domains[p2])
                return true;

            return HasConnection(domains[p1], domains[p2]);
        }

        bool HasConnection(int d1,int d2)
        {
            return false;
        }
    }
}
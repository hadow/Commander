using System;
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

        }

        public bool IsPassable(CPos p1,CPos p2,uint movementClass)
        {
            return domainIndexes[movementClass].IsPassable(p1, p2); 
        }

    }

    class MovementClassDomainIndex
    {
        readonly Map map;
        readonly uint movementClass;
        readonly CellLayer<int> domains;
        readonly Dictionary<int, HashSet<int>> transientConnections;



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
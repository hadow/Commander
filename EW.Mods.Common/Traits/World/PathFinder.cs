using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Mods.Common.Pathfinder;
namespace EW.Mods.Common.Traits
{

    public interface IPathFinder
    {
        /// <summary>
        /// Calculates a path for the actor from source to destination
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="self"></param>
        /// <returns></returns>
        List<CPos> FindUnitPath(CPos source, CPos target, Actor self);

        List<CPos> FindUnitPathToRange(CPos source, SubCell surSub, WPos target, WDist range, Actor self);

        /// <summary>
        /// Calculates a path given a search specification
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        List<CPos> FindPath(IPathSearch search);
        List<CPos> FindBidiPath(IPathSearch fromSrc, IPathSearch fromDest);
    }

    /// <summary>
    /// Calculates routes for mobile units based on the A* search algorithm.
    /// </summary>
    public class PathFinderInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new PathFinder(init.World); }
    }

    public class PathFinder:IPathFinder
    {
        static readonly List<CPos> EmptyPath = new List<CPos>(0);
        readonly World world;

        public PathFinder(World world)
        {
            this.world = world;
        }

        public List<CPos> FindUnitPath(CPos source,CPos target,Actor self)
        {
            var mi = self.Info.TraitInfo<MobileInfo>();

            var domainIndex = world.WorldActor.TraitOrDefault<DomainIndex>();

            if (domainIndex != null)
            {
                var passable = mi.GetMovementClass(world.Map.Rules.TileSet);
                if (!domainIndex.IsPassable(source, target, (uint)passable))
                    return EmptyPath;
            }

            List<CPos> pb;
            using (var fromSrc = PathSearch.FromPoint(world, mi, self, target, source, true))
            using (var fromDest = PathSearch.FromPoint(world, mi, self, source, target, true).Reverse())
                pb = FindBidiPath(fromSrc, fromDest);
        }

        public List<CPos> FindBidiPath(IPathSearch fromSrc,IPathSearch fromDest)
        {
            return EmptyPath;
        }
    }
}
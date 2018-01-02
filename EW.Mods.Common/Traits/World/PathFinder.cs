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
        List<CPos> FindUnitPath(CPos source, CPos target, Actor self,Actor ignoreActor);

        List<CPos> FindUnitPathToRange(CPos source, SubCell surSub, WPos target, WDist range, Actor self);

        /// <summary>
        /// Calculates a path given a search specification
        /// 计算给定搜索规范的路径
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        List<CPos> FindPath(IPathSearch search);
        List<CPos> FindBidiPath(IPathSearch fromSrc, IPathSearch fromDest);
    }

    /// <summary>
    /// Calculates routes for mobile units based on the A* search algorithm.
    /// 根据A *搜索算法计算移动单元的路线
    /// </summary>
    public class PathFinderInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new PathFinderUnitPathCacheDecorator(new PathFinder(init.World),new PathCacheStorage(init.World)); }
    }

    public class PathFinder:IPathFinder
    {
        static readonly List<CPos> EmptyPath = new List<CPos>(0);
        readonly World world;

        public PathFinder(World world)
        {
            this.world = world;
        }

        public List<CPos> FindUnitPath(CPos source,CPos target,Actor self,Actor ignoreActor)
        {
            var mi = self.Info.TraitInfo<MobileInfo>();

            //if water-land transition is required,bail early
            var domainIndex = world.WorldActor.TraitOrDefault<DomainIndex>();

            if (domainIndex != null)
            {
                var passable = mi.GetMovementClass(world.Map.Rules.TileSet);
                if (!domainIndex.IsPassable(source, target, (uint)passable))
                    return EmptyPath;
            }

            List<CPos> pb;
            using (var fromSrc = PathSearch.FromPoint(world, mi, self, target, source, true).WithIgnoredActor(ignoreActor))
            using (var fromDest = PathSearch.FromPoint(world, mi, self, source, target, true).WithIgnoredActor(ignoreActor).Reverse())
                pb = FindBidiPath(fromSrc, fromDest);

            CheckStanePath2(pb,source,target);

            return pb;

        }

        public List<CPos> FindBidiPath(IPathSearch fromSrc,IPathSearch fromDest)
        {
            return EmptyPath;
        }

        public List<CPos> FindPath(IPathSearch search)
        {
            return EmptyPath;
        }

        public List<CPos> FindUnitPathToRange(CPos source,SubCell srcSub,WPos target,WDist range,Actor self)
        {

            var mi = self.Info.TraitInfo<MobileInfo>();
            var targetCell = world.Map.CellContaining(target);

            target -= world.Map.Grid.OffsetOfSubCell(srcSub);

            var tilesInRange = world.Map.f
        }


        static void CheckStanePath2(IList<CPos> path,CPos src,CPos dest){
            if (path.Count == 0)
                return;

            if (path[0] != dest)
                throw new InvalidOperationException("(PathFinder) sanity check failed:doesn't go to dest");

            if (path[path.Count - 1] != src)
                throw new InvalidOperationException("(PathFinder) sanity check failed:doesn't come from src.");
            
        }
    }
}
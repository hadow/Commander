using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Mods.Common.Pathfinder;
using System.Linq;
using System.Diagnostics;
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
                if (!domainIndex.IsPassable(source, target,mi,(uint)passable))
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

            List<CPos> path = null;

            while(fromSrc.CanExpand && fromDest.CanExpand)
            {
                //make some progress on the first search
                var p = fromSrc.Expand();

                if(fromDest.Graph[p].Status == CellStatus.Closed && fromDest.Graph[p].CostSoFar < int.MaxValue)
                {
                    path = MakeBidiPath(fromSrc, fromDest, p);
                    break;
                }


                //make some progress on the second search
                var q = fromDest.Expand();

                if(fromSrc.Graph[q].Status == CellStatus.Closed && fromSrc.Graph[q].CostSoFar < int.MaxValue)
                {
                    path = MakeBidiPath(fromSrc, fromDest, q);
                    break;
                }

            }
            fromSrc.Graph.Dispose();
            fromDest.Graph.Dispose();

            if (path != null)
                return path;
            return EmptyPath;
        }

        public List<CPos> FindPath(IPathSearch search)
        {

            List<CPos> path = null;

            while (search.CanExpand)
            {
                var p = search.Expand();
                if (search.IsTarget(p))
                {
                    path = MakePath(search.Graph, p);
                    break;
                }
            }

            search.Graph.Dispose();
            if(path != null)
                return path;

            return EmptyPath;
        }

        public List<CPos> FindUnitPathToRange(CPos source,SubCell srcSub,WPos target,WDist range,Actor self)
        {

            var mi = self.Info.TraitInfo<MobileInfo>();
            var targetCell = world.Map.CellContaining(target);

            //Correct for SubCell offset.
            //纠正SubCell偏移
            target -= world.Map.Grid.OffsetOfSubCell(srcSub);

            var tilesInRange = world.Map.FindTilesInCircle(targetCell, range.Length / 1024 + 1).
                Where(t => (world.Map.CenterOfCell(t) - target).LengthSquared <= range.LengthSquared && mi.CanEnterCell(self.World, self, t));

            var domainIndex = world.WorldActor.TraitOrDefault<DomainIndex>();

            if(domainIndex != null)
            {
                var passable = mi.GetMovementClass(world.Map.Rules.TileSet);
                tilesInRange = new List<CPos>(tilesInRange.Where(t => domainIndex.IsPassable(source, t, mi, (uint)passable)));
                if (!tilesInRange.Any())
                    return EmptyPath;
            }

            using (var fromSrc = PathSearch.FromPoints(world, mi, self, tilesInRange, source, true))
            using (var fromDest = PathSearch.FromPoint(world, mi, self, source, targetCell, true).Reverse())
                return FindBidiPath(fromSrc, fromDest);

        }


        [Conditional("SANITY_CHECKS")]
        static void CheckSanePath(IList<CPos> path)
        {
            if (path.Count == 0)
                return;
            var prev = path[0];
            foreach(var cell in path)
            {
                var d = cell - prev;

                if (Math.Abs(d.X) > 1 || Math.Abs(d.Y) > 1)
                    throw new InvalidOperationException("(PathFinder) path sanity check failed");

                prev = cell;
            }
        }


        [Conditional("SANITY_CHECKS")]
        static void CheckStanePath2(IList<CPos> path,CPos src,CPos dest){
            if (path.Count == 0)
                return;

            if (path[0] != dest)
                throw new InvalidOperationException("(PathFinder) sanity check failed:doesn't go to dest");

            if (path[path.Count - 1] != src)
                throw new InvalidOperationException("(PathFinder) sanity check failed:doesn't come from src.");
            
        }


        static List<CPos> MakePath(IGraph<CellInfo> cellInfo,CPos destination)
        {
            var ret = new List<CPos>();
            var currentNode = destination;

            while(cellInfo[currentNode].PreviousPos != currentNode)
            {
                ret.Add(currentNode);
                currentNode = cellInfo[currentNode].PreviousPos;

            }
            ret.Add(currentNode);
            CheckSanePath(ret);

            return ret;
        }


        static List<CPos> MakeBidiPath(IPathSearch a,IPathSearch b,CPos conflueneNode)
        {

            var ca = a.Graph;
            var cb = b.Graph;

            var ret = new List<CPos>();

            var q = conflueneNode;
            while(ca[q].PreviousPos != q)
            {
                ret.Add(q);
                q = ca[q].PreviousPos;
            }

            ret.Add(q);

            ret.Reverse();

            q = conflueneNode;

            while(cb[q].PreviousPos != q)
            {
                q = cb[q].PreviousPos;
                ret.Add(q);

                
            }
            CheckSanePath(ret);
            return ret;
        }
    }
}
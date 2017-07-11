using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Mods.Common.Pathfinder;
namespace EW.Mods.Common.Traits
{

    public interface IPathFinder
    {
        List<CPos> FindUnitPath(CPos source, CPos target, Actor self);

        List<CPos> FindUnitPathToRange(CPos source, SubCell surSub, WPos target, WDist range, Actor self);

        List<CPos> FindPath(IPathSearch search);
        List<CPos> FindPath(IPathSearch fromSrc, IPathSearch fromDest);
    }


    public class PathFinderInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new PathFinder(); }
    }

    public class PathFinder
    {
    }
}
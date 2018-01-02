using System;
using System.Collections.Generic;
using EW.Mods.Common.Traits;
using EW.Support;
using EW.Traits;
namespace EW.Mods.Common.Pathfinder
{
    public class PathFinderUnitPathCacheDecorator:IPathFinder
    {

        readonly IPathFinder pathFinder;
        readonly ICacheStorage<List<CPos>> cacheStorage;
        public PathFinderUnitPathCacheDecorator(IPathFinder pathFinder,ICacheStorage<List<CPos>> cacheStorage)
        {
            this.pathFinder = pathFinder;
            this.cacheStorage = cacheStorage;


        }


        public List<CPos> FindPath(IPathSearch search){
            using(new PerfSample("PathFinder")){
                return pathFinder.FindPath(search);
            }
        }

        public List<CPos> FindUnitPath(CPos source,CPos target,Actor self,Actor ignoreActor){

            using(new PerfSample("Pathfinder")){

                var key = "FindUnitPath" + self.ActorID + source.X + source.Y + target.X + target.Y;

                var cachedPath = cacheStorage.Retrieve(key);

                if (cachedPath != null)
                    return cachedPath;

                var pb = pathFinder.FindUnitPath(source, target, self, ignoreActor);

                cacheStorage.Store(key,pb);

                return pb;
            }
        }


        public List<CPos> FindUnitPathToRange(CPos source,SubCell srcSub,WPos target,WDist range,Actor self){
            using(new PerfSample("PathFinder")){

                var key = "FindUnitPathToRange" + self.ActorID + source.X + source.Y + target.X + target.Y;

                var cachedPath = cacheStorage.Retrieve(key);

                if (cachedPath != null)
                    return cachedPath;

                var pb = pathFinder.FindUnitPathToRange(source, srcSub, target, range, self);

                cacheStorage.Store(key,pb);
                return pb;
            }
        }

        public List<CPos> FindBidiPath(IPathSearch fromSrc,IPathSearch fromDest){
            using(new PerfSample("Pathfinder")){
                return pathFinder.FindBidiPath(fromSrc, fromDest);
            }
        }

    }
}

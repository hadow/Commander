using System;
using System.Collections.Generic;
using System.Linq;
namespace EW.Mods.Common.Pathfinder
{
    public class PathCacheStorage:ICacheStorage<List<CPos>>
    {

        class CachedPath{

            public List<CPos> Result;
            public int Tick;

        }

        const int MaxPathAge = 50;

        readonly World world;

        Dictionary<string, CachedPath> cachedPaths = new Dictionary<string, CachedPath>(100);


        public PathCacheStorage(World world)
        {
            this.world = world;

        }

        public void Remove(string key){
            cachedPaths.Remove((key));
        }

        public void Store(string key,List<CPos> data){



            if(cachedPaths.Count>=100){

                foreach(var cachedPath in cachedPaths.Where(p=>isExpired(p.Value)).ToList()){
                    cachedPaths.Remove(cachedPath.Key);
                }


            }

            cachedPaths.Add(key,new CachedPath{

                Tick = world.WorldTick,
                Result = data,

            });
        }

        public List<CPos> Retrieve(string key){

            CachedPath cached;
            if(cachedPaths.TryGetValue(key,out cached)){
                if(isExpired(cached)){
                    cachedPaths.Remove(key);
                    return null;
                }

                return cached.Result;
            }
            return null;
        }

        /// <summary>
        /// 生命期
        /// </summary>
        /// <returns><c>true</c>, if expired was ised, <c>false</c> otherwise.</returns>
        /// <param name="path">Path.</param>
        bool isExpired(CachedPath path){
            return world.WorldTick - path.Tick > MaxPathAge;
        }
    }
}

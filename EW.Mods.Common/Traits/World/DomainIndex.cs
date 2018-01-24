using System;
using System.Linq;
using System.Collections.Generic;
using EW.Graphics;
using EW.Traits;
using EW.Support;
namespace EW.Mods.Common.Traits
{

    /// <summary>
    /// Identify untraversable regions of the map for faster pathfinding,especially with AI.
    /// 识别地图的不可逆区域，以便更快地进行寻路，尤其是使用AI。
    /// </summary>
    public class DomainIndexInfo : TraitInfo<DomainIndex> { }


    public class DomainIndex:IWorldLoaded
    {
        Dictionary<uint, MovementClassDomainIndex> domainIndexes;

        public void WorldLoaded(World world,WorldRenderer wr)
        {
            domainIndexes = new Dictionary<uint, MovementClassDomainIndex>();

            var tileSet = world.Map.Rules.TileSet;

            var movementClasses = world.Map.Rules.Actors.Where(ai => 
                                                               ai.Value.HasTraitInfo<MobileInfo>()).Select(ai=> (uint)ai.Value.TraitInfo<MobileInfo>().GetMovementClass(tileSet)).Distinct();

            foreach(var mc in movementClasses)
            {
                domainIndexes[mc] = new MovementClassDomainIndex(world, mc);
            }
        }

        public bool IsPassable(CPos p1,CPos p2,MobileInfo mi,uint movementClass)
        {
            if (p1.Layer != 0 || p2.Layer != 0)
                return true;

            if (mi.Subterranean || mi.Jumpjet)
                return true;
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
        readonly CellLayer<ushort> domains;
        readonly Dictionary<ushort, HashSet<ushort>> transientConnections;

        public MovementClassDomainIndex(World world,uint movementClass)
        {
            map = world.Map;
            this.movementClass = movementClass;
            domains = new CellLayer<ushort>(world.Map);
            transientConnections = new Dictionary<ushort, HashSet<ushort>>();

            using (new PerfTimer("BuildDomains:{0} for movement class {1}".F(world.Map.Title, movementClass)))
                BuildDomains(world);
        }

        public bool IsPassable(CPos p1,CPos p2)
        {
            if (!domains.Contains(p1) || !domains.Contains(p2))
                return false;

            if (domains[p1] == domains[p2])
                return true;

            return HasConnection(domains[p1], domains[p2]);
        }

        public void UpdateCells(World world,HashSet<CPos> dirtyCells){

            var neighborDomains = new List<ushort>();

            foreach(var cell in dirtyCells){

                var thisCell = cell;
                var neighbors = CVec.Directions.Select(d => d + thisCell).Where(c => map.Contains(c));

                var found = false;

                foreach(var n in neighbors){
                    if(!dirtyCells.Contains(n)){


                        var neighborDomain = domains[n];
                        if(CanTraverseTile(world,n)){

                            neighborDomains.Add(neighborDomain);

                            if(!found){
                                domains[cell] = neighborDomain;
                                found = true;
                            }
                        }
                    }
                }
            }

            foreach (var c1 in neighborDomains)
                foreach (var c2 in neighborDomains)
                    CreateConnection(c1, c2);
        }

        /// <summary>
        /// Hases the connection.
        /// </summary>
        /// <returns><c>true</c>, if connection was hased, <c>false</c> otherwise.</returns>
        /// <param name="d1">D1.</param>
        /// <param name="d2">D2.</param>
        bool HasConnection(ushort d1,ushort d2)
        {

            //Search our connections graph for a possible  route.
            var visited = new HashSet<ushort>();
            var toProcess = new Stack<ushort>();
            toProcess.Push(d1);

            while(toProcess.Any()){
                var current = toProcess.Pop();
                if(!transientConnections.ContainsKey(current)){
                    continue;
                }


                foreach(var neighbor in transientConnections[current]){
                    if (neighbor == d2)
                        return true;

                    if (!visited.Contains(neighbor))
                        toProcess.Push(neighbor);


                }

                visited.Add(current);
            }
            return false;
        }


        public void AddFixedConnection(IEnumerable<CPos> cells){

            var cellDomains = cells.Select(c => domains[c]).ToHashSet();
            foreach(var c1 in cellDomains){
                foreach (var c2 in cellDomains.Where(c => c != c1))
                    CreateConnection(c1, c2);
            }
        }

        /// <summary>
        /// Creates the connection.
        /// </summary>
        /// <param name="d1">D1.</param>
        /// <param name="d2">D2.</param>
        void CreateConnection(ushort d1,ushort d2){


            if (!transientConnections.ContainsKey(d1))
                transientConnections[d1] = new HashSet<ushort>();

            if (!transientConnections.ContainsKey(d2))
                transientConnections[d2] = new HashSet<ushort>();

            transientConnections[d1].Add(d2);
            transientConnections[d2].Add(d1);
            
        }

        /// <summary>
        /// Cans the traverse tile.
        /// </summary>
        /// <returns><c>true</c>, if traverse tile was caned, <c>false</c> otherwise.</returns>
        /// <param name="world">World.</param>
        /// <param name="p">P.</param>
        bool CanTraverseTile(World world,CPos p){

            if (!map.Contains(p))
                return false;

            var terrainOffset = world.Map.GetTerrainIndex(p);

            return (movementClass & (1 << terrainOffset)) > 0;
        }
        
        /// <summary>
        /// Builds the domains.
        /// 构建域
        /// </summary>
        /// <param name="world">World.</param>
        void BuildDomains(World world){
            ushort domain = 1;

            var visited = new CellLayer<bool>(map);

            var toProcess = new Queue<CPos>();
            toProcess.Enqueue(MPos.Zero.ToCPos(map));

            //Flood-fill over  each domain.
            //填充每个域
            while(toProcess.Count != 0){

                var start = toProcess.Dequeue();

                //Technically redundant with the check in the inner loop,
                //but prevents ballooning the domain counter
                //技术上在内部循环检查冗余，防止膨胀域计数器
                if (visited[start])
                    continue;

                var domainQueue = new Queue<CPos>();
                domainQueue.Enqueue(start);


                var currentPassable = CanTraverseTile(world, start);


                //Add all contiguous cells to our domain,and make a not of an non-contiguous cells for future domain.
                //将所有连续的单元格添加到我们的域，并记录下未来域的非连续单元格
                while(domainQueue.Count!=0){

                    var n = domainQueue.Dequeue();
                    if (visited[n])
                        continue;

                    var candidatePassable = CanTraverseTile(world, n);
                    if(candidatePassable != currentPassable)
                    {
                        toProcess.Enqueue(n);
                        continue;

                    }

                    visited[n] = true;
                    domains[n] = domain;


                    //PERF:Avoid LINQ
                    foreach(var direction in CVec.Directions){

                        //Don't crawl off the map ,or add already-visited cells.
                        //不要抓取地图，或添加已经访问的单元格
                        var neighbor = direction + n;
                        if (visited.Contains(neighbor) && !visited[neighbor])
                            domainQueue.Enqueue(neighbor);
                    }

                }

                domain += 1;


            }
        }
    }
}
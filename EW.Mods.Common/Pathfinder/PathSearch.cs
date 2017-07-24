using System;
using System.Collections.Generic;
using EW.Mods.Common.Traits;
using System.Runtime.CompilerServices;
using EW.Primitives;
namespace EW.Mods.Common.Pathfinder
{
    /// <summary>
    /// 搜寻路径
    /// </summary>
    public sealed class PathSearch:BasePathSearch
    {
        LinkedList<Pair<CPos, int>> considered;

        public override IEnumerable<Pair<CPos, int>> Considered { get { return considered; } }

        private PathSearch(IGraph<CellInfo> graph) : base(graph)
        {
            considered = new LinkedList<Pair<CPos, int>>();
        }

        //PERF:Maintain a pool of layers used for paths searches for each world.These searches are performed often 
        //so we wish to avoid the high cost of initializing a new search space every time by reusing the old ones.
        static readonly ConditionalWeakTable<World, CellInfoLayerPool> LayerPoolTable = new ConditionalWeakTable<World, CellInfoLayerPool>();
        static readonly ConditionalWeakTable<World, CellInfoLayerPool>.CreateValueCallback CreateLayerPool = world => new CellInfoLayerPool(world.Map);

        static CellInfoLayerPool LayerPoolForWorld(World world)
        {
            return LayerPoolTable.GetValue(world, CreateLayerPool);
        }


        public static IPathSearch FromPoint(World world,MobileInfo mi,Actor self,CPos from,CPos target,bool checkForBlocked)
        {
            var graph = new PathGraph(LayerPoolForWorld(world), mi, self, world, checkForBlocked);
            var search = new PathSearch(graph)
            {
                heuristic = DefaultEstimator(target),
                
           
            };
            search.isGoal = loc => {
                var locInfo = search.Graph[loc];
                return locInfo.EstimatedTotal - locInfo.CostSoFar == 0;
                };
            if (world.Map.Contains(from))
                search.AddInitialCell(from);
            return search;
        }


        protected override void AddInitialCell(CPos location)
        {
            var cost = heuristic(location);
            Graph[location] = new CellInfo(0, cost, location, CellStatus.Open);
            var connection = new GraphConnection(location, cost);
            OpenQueue.Add(connection);
            StartPoints.Add(connection);
            considered.AddLast(new Pair<CPos, int>(location, 0));
        }

        /// <summary>
        /// This function analyzes the neighbors of the most promising node in the Pathfinding graph
        /// using the A* algorithm (A-Star) and returns that node
        /// </summary>
        /// <returns>The most promising node of the iteration</returns>
        public override CPos Expand()
        {
            var currentMinNode = OpenQueue.Pop().Destination;

            var currentCell = Graph[currentMinNode];
            Graph[currentMinNode] = new CellInfo(currentCell.CostSoFar, currentCell.EstimatedTotal, currentCell.PreviousPos, CellStatus.Closed);

            if (Graph.CustomCost != null && Graph.CustomCost(currentMinNode) == Constants.InvalidNode)
                return currentMinNode;

            foreach(var connection in Graph.GetConnections(currentMinNode))
            {
                var gCost = currentCell.CostSoFar + connection.Cost;

                var neighborCPos = connection.Destination;
                var neighborCell = Graph[neighborCPos];

                // Cost is even higher;next direction;
                if (neighborCell.Status == CellStatus.Closed || gCost >= neighborCell.CostSoFar)
                    continue;

                int hCost;
                if (neighborCell.Status == CellStatus.Open)
                    hCost = neighborCell.EstimatedTotal - neighborCell.CostSoFar;
                else
                    hCost = heuristic(neighborCPos);

                var estimatedCost = gCost + hCost;
                Graph[neighborCPos] = new CellInfo(gCost, estimatedCost, currentMinNode, CellStatus.Open);

                if (neighborCell.Status != CellStatus.Open)
                    OpenQueue.Add(new GraphConnection(neighborCPos, estimatedCost));
            }

            return currentMinNode;

        }



    }
}
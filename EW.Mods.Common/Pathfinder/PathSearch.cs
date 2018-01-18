using System;
using System.Collections.Generic;
using EW.Mods.Common.Traits;
using System.Runtime.CompilerServices;
using EW.Primitives;
using System.Linq;
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
        //维护用于路径搜索的每个世界的图层池。这些搜索经常执行，所以我们希望避免每次重新使用旧搜索空间的初始化新的搜索空间的成本。
        static readonly ConditionalWeakTable<World, CellInfoLayerPool> LayerPoolTable = new ConditionalWeakTable<World, CellInfoLayerPool>();

        static readonly ConditionalWeakTable<World, CellInfoLayerPool>.CreateValueCallback CreateLayerPool = world => new CellInfoLayerPool(world.Map);

        static CellInfoLayerPool LayerPoolForWorld(World world)
        {
            return LayerPoolTable.GetValue(world, CreateLayerPool);
        }


        public static IPathSearch Search(World world,MobileInfo mi,Actor self,bool checkForBlocked,Func<CPos,bool> goalCondition){

            var graph = new PathGraph(LayerPoolForWorld(world), mi, self, world, checkForBlocked);
            var search = new PathSearch(graph);
            search.isGoal = goalCondition;
            search.heuristic = loc => 0;
            return search;
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

        public static IPathSearch FromPoints(World world,MobileInfo mi,Actor self,IEnumerable<CPos> froms,CPos target,bool checkForBlocked)
        {
            var graph = new PathGraph(LayerPoolForWorld(world), mi, self, world, checkForBlocked);
            var search = new PathSearch(graph)
            {
                heuristic = DefaultEstimator(target)
            };

            search.isGoal = loc =>
            {
                var locInfo = search.Graph[loc];
                return locInfo.EstimatedTotal - locInfo.CostSoFar == 0;
            };


            foreach (var sl in froms.Where(sl => world.Map.Contains(sl)))
                search.AddInitialCell(sl);


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
        /// 该函数使用A* 算法 分析 Pathfinding图中最有希望的节点的邻居，并返回该节点。 
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

                // Now we may seriously consider this direction using heuristics.If the cell has already been processed,
                //we can reuse the result (just the difference between the estimated total and the cost so far)
                //现在我们可以用启发式的方法认真考虑这个方向。如果这个单元格已经被处理了，我们可以重新使用这个结果（只是估计的总数和成本之间的差距）
                int hCost;
                if (neighborCell.Status == CellStatus.Open)
                    hCost = neighborCell.EstimatedTotal - neighborCell.CostSoFar;
                else
                    hCost = heuristic(neighborCPos);

                var estimatedCost = gCost + hCost;
                Graph[neighborCPos] = new CellInfo(gCost, estimatedCost, currentMinNode, CellStatus.Open);

                if (neighborCell.Status != CellStatus.Open)
                    OpenQueue.Add(new GraphConnection(neighborCPos, estimatedCost));

                if(Debug){
                    if (gCost > MaxCost)
                        gCost = MaxCost;

                    considered.AddLast(new Pair<CPos, int>(neighborCPos,gCost));
                }
            }

            return currentMinNode;

        }



    }
}
using System;
using System.Collections.Generic;
using EW.Mods.Common.Traits;
using System.Runtime.CompilerServices;
using EW.Primitives;
namespace EW.Mods.Common.Pathfinder
{
    public sealed class PathSearch:BasePathSearch
    {
        LinkedList<Pair<CPos, int>> considered;

        public override IEnumerable<Pair<CPos, int>> Considered { get { return considered; } }

        private PathSearch(IGraph<CellInfo> graph) : base(graph)
        {
            considered = new LinkedList<Pair<CPos, int>>();
        }
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

        protected override void AddInitialCell(CPos cell)
        {
            throw new NotImplementedException();
        }


    }
}
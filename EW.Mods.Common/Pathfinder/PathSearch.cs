using System;
using EW.Mods.Common.Traits;
using System.Runtime.CompilerServices;
namespace EW.Mods.Common.Pathfinder
{
    public sealed class PathSearch:BasePathSearch
    {

        private PathSearch(IGraph<CellInfo> graph) : base(graph)
        {

        }
        static readonly ConditionalWeakTable<World, CellInfoLayerPool> LayerPoolTable = new ConditionalWeakTable<World, CellInfoLayerPool>();
        static readonly ConditionalWeakTable<World, CellInfoLayerPool>.CreateValueCallback CreateLayerPool = world => new CellInfoLayerPool();

        static CellInfoLayerPool LayerPoolForWorld(World world)
        {
            return LayerPoolTable.GetValue(world, CreateLayerPool);
        }
        public static IPathSearch FromPoint(World world,MobileInfo mi,Actor self,CPos from,CPos target,bool checkForBlocked)
        {
            var graph = new PathGraph(LayerPoolForWorld(world), mi, self, world, checkForBlocked);
            var search = new PathSearch(graph);

            return null;
        }


    }
}
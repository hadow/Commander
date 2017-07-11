using System;
using System.Collections.Generic;
using EW.Mods.Common.Traits;

namespace EW.Mods.Common.Pathfinder
{
    public struct GraphConnection
    {
        public sealed class CostComparer : IComparer<GraphConnection>
        {
            public static readonly CostComparer Instance = new CostComparer();

            public int Compare(GraphConnection x,GraphConnection y)
            {
                return x.Cost.CompareTo(y.Cost);
            }
        }

        public static readonly CostComparer ConnectionCostComparer = CostComparer.Instance;

        public readonly CPos Destination;
        public readonly int Cost;

        public GraphConnection(CPos destination,int cost)
        {
            Destination = destination;
            Cost = cost;
        }
    }
    public interface IGraph<T> : IDisposable
    {
        List<GraphConnection> GetConnections(CPos position);

        T this[CPos pos] { get;set; }

        Func<CPos,bool> CustomBlock { get; set; }

        Func<CPos,int> CustomCost { get; set; }

        int LaneBias { get; set; }

        bool InReverse { get; set; }

        Actor IgnoredActor { get; set; }

        World World { get; }

        Actor Actor { get; }
    }

    sealed class PathGraph:IGraph<CellInfo>
    {
        public Actor Actor { get; private set; }

        public World World { get; private set; }

        public Func<CPos,bool> CustomBlock { get; set; }

        public Func<CPos,int> CustomCost { get; set; }

        public int LaneBias { get; set; }

        public bool InReverse { get; set; }

        public Actor IgnoredActor { get; set; }

        CellLayer<CellInfo> cellInfo;

        public PathGraph(CellInfoLayerPool layerPool,MobileInfo mobileInfo,Actor actor,World world,bool checkForBlocked)
        {


        }
        public List<GraphConnection> GetConnections(CPos position)
        {
            var validNeighbors = new List<GraphConnection>();
            return validNeighbors;
        }


        public CellInfo this[CPos pos]
        {
            get { return cellInfo[pos]; }
            set
            {
                cellInfo[pos] = value;
            }
        }


        public void Dispose()
        {

        }

    }
}
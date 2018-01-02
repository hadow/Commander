using System;
using System.Collections.Generic;
using EW.Mods.Common.Traits;
using EW.Primitives;
using System.Linq;
namespace EW.Mods.Common.Pathfinder
{

    /// <summary>
    /// tip:Reduce size of GraphConnection for allocation efficiency.
    /// </summary>
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

    /// <summary>
    /// 
    /// </summary>
    sealed class PathGraph:IGraph<CellInfo>
    {
        public Actor Actor { get; private set; }

        public World World { get; private set; }

        public Func<CPos,bool> CustomBlock { get; set; }

        public Func<CPos,int> CustomCost { get; set; }

        public int LaneBias { get; set; }

        public bool InReverse { get; set; }

        public Actor IgnoredActor { get; set; }

        readonly CellConditions checkConditions;

        readonly bool checkTerrainHeight;

        
        readonly MobileInfo mobileInfo;

        readonly MobileInfo.WorldMovementInfo worldMovementInfo;

        readonly CellInfoLayerPool.PooledCellInfoLayer pooledLayer;

        CellLayer<CellInfo> groundInfo;

        readonly Dictionary<byte, Pair<ICustomMovementLayer, CellLayer<CellInfo>>> customLayerInfo = new Dictionary<byte, Pair<ICustomMovementLayer, CellLayer<CellInfo>>>();


        public PathGraph(CellInfoLayerPool layerPool,MobileInfo mobileInfo,Actor actor,World world,bool checkForBlocked)
        {
            pooledLayer = layerPool.Get();
            groundInfo = pooledLayer.GetLayer();

            var layers = world.GetCustomMovementLayers().Values.Where(cml => cml.EnabledForActor(actor.Info, mobileInfo));

            foreach (var cml in layers)
                customLayerInfo[cml.Index] = Pair.New(cml, pooledLayer.GetLayer());
            
            World = world;
            this.mobileInfo = mobileInfo;
            worldMovementInfo = mobileInfo.GetWorldMovementInfo(world);
            Actor = actor;
            LaneBias = 1;
            checkConditions = checkForBlocked ? CellConditions.TransientActors:CellConditions.None;
            checkTerrainHeight = world.Map.Grid.MaximumTerrainHeight > 0;
        }

        static readonly CVec[][] DirectedNeighbors =
        {
            new[]{new CVec(-1,-1),new CVec(0,-1),new CVec(1,-1),new CVec(-1,0),new CVec(-1,1)},
            new[]{new CVec(-1,-1),new CVec(0,-1),new CVec(1,-1)},
            new[]{new CVec(-1,-1),new CVec(0,-1),new CVec(1,-1),new CVec(1,0),new CVec(1,1)},
            new[]{new CVec(-1,-1),new CVec(-1,0),new CVec(-1,1)},
            CVec.Directions,
            new[]{new CVec(1,-1),new CVec(1,0),new CVec(1,1)},
            new[]{new CVec(-1,-1),new CVec(-1,0),new CVec(-1,1),new CVec(0,1),new CVec(1,1)},
            new[]{new CVec(-1,1),new CVec(0,1),new CVec(1,1)},
            new[]{new CVec(1,-1),new CVec(1,0),new CVec(-1,1),new CVec(0,1),new CVec(1,1)}

        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public List<GraphConnection> GetConnections(CPos position)
        {
            var previousPos = groundInfo[position].PreviousPos;

            var dx = position.X - previousPos.X;
            var dy = position.Y - previousPos.Y;

            var index = dy * 3 + dx + 4;

            var directions = DirectedNeighbors[index];

            var validNeighbors = new List<GraphConnection>(directions.Length);
            for(var i = 0; i < directions.Length; i++)
            {
                var neighbor = position + directions[i];
                var movementCost = GetCostToNode(neighbor, directions[i]);
                if (movementCost != Constants.InvalidNode)
                    validNeighbors.Add(new GraphConnection(neighbor, movementCost));
            }
            return validNeighbors;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="destNode"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        int GetCostToNode(CPos destNode,CVec direction)
        {
            var movementCost = mobileInfo.MovementCostToEnterCell(worldMovementInfo, Actor, destNode, IgnoredActor, checkConditions);
            if (movementCost != int.MaxValue && !(CustomBlock != null && CustomBlock(destNode)))
                return CalculateCellCost(destNode, direction, movementCost);

            return Constants.InvalidNode;
        }

        int CalculateCellCost(CPos neighborCPos,CVec direction,int movementCost)
        {
            var cellCost = movementCost;

            if (direction.X * direction.Y != 0)
                cellCost = (cellCost * 34) / 24;

            if(CustomCost != null)
            {
                var customCost = CustomCost(neighborCPos);
                if (customCost == Constants.InvalidNode)
                    return Constants.InvalidNode;

                cellCost += customCost;

            }
            return cellCost;
        }

        public CellInfo this[CPos pos]
        {
            get { return groundInfo[pos]; }
            set
            {
                groundInfo[pos] = value;
            }
        }


        public void Dispose()
        {
            customLayerInfo.Clear();
            pooledLayer.Dispose();
            groundInfo = null;
        }

    }
}
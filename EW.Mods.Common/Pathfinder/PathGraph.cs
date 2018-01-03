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

    /// <summary>
    /// Represents a graph with nodes and edges
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGraph<T> : IDisposable
    {
        /// <summary>
        /// Gets all the Connections for a given node in the graph.
        /// 获取图中给定节点的所有连接
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        List<GraphConnection> GetConnections(CPos position);

        /// <summary>
        /// Retrieves an object given a node in the graph.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        T this[CPos pos] { get;set; }

        Func<CPos,bool> CustomBlock { get; set; }

        Func<CPos,int> CustomCost { get; set; }

        int LaneBias { get; set; }

        bool InReverse { get; set; }

        Actor IgnoreActor { get; set; }

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

        public Actor IgnoreActor { get; set; }

        readonly CellConditions checkConditions;

        readonly bool checkTerrainHeight;       //检查地形高度

        
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
            new[]{new CVec(-1,-1),new CVec(0,-1),new CVec(1,-1),new CVec(-1,0),new CVec(-1,1)}, //0
            new[]{new CVec(-1,-1),new CVec(0,-1),new CVec(1,-1)},                               //1
            new[]{new CVec(-1,-1),new CVec(0,-1),new CVec(1,-1),new CVec(1,0),new CVec(1,1)},   //2
            new[]{new CVec(-1,-1),new CVec(-1,0),new CVec(-1,1)},                               //3
            CVec.Directions,                                                                    //4
            new[]{new CVec(1,-1),new CVec(1,0),new CVec(1,1)},                                  //5
            new[]{new CVec(-1,-1),new CVec(-1,0),new CVec(-1,1),new CVec(0,1),new CVec(1,1)},   //6
            new[]{new CVec(-1,1),new CVec(0,1),new CVec(1,1)},                                  //7
            new[]{new CVec(1,-1),new CVec(1,0),new CVec(-1,1),new CVec(0,1),new CVec(1,1)}      //8

        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public List<GraphConnection> GetConnections(CPos position)
        {
            var info = position.Layer == 0 ? groundInfo : customLayerInfo[position.Layer].Second;
            var previousPos = info[position].PreviousPos;

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

            if(position.Layer == 0)
            {
                foreach(var cli in customLayerInfo.Values)
                {
                    var layerPosition = new CPos(position.X, position.Y, cli.First.Index);
                    var entryCost = cli.First.EntryMovementCost(Actor.Info, mobileInfo, layerPosition);
                    if (entryCost != Constants.InvalidNode)
                        validNeighbors.Add(new GraphConnection(layerPosition, entryCost));
                }
            }
            else
            {
                var layerPosition = new CPos(position.X, position.Y, 0);
                var exitCost = customLayerInfo[position.Layer].First.ExitMovementCost(Actor.Info, mobileInfo, layerPosition);
                if (exitCost != Constants.InvalidNode)
                    validNeighbors.Add(new GraphConnection(layerPosition, exitCost));
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
            var movementCost = mobileInfo.MovementCostToEnterCell(worldMovementInfo, Actor, destNode, IgnoreActor, checkConditions);
            if (movementCost != int.MaxValue && !(CustomBlock != null && CustomBlock(destNode)))
                return CalculateCellCost(destNode, direction, movementCost);

            return Constants.InvalidNode;
        }

        /// <summary>
        /// 计算近邻单元格成本
        /// </summary>
        /// <param name="neighborCPos"></param>
        /// <param name="direction"></param>
        /// <param name="movementCost"></param>
        /// <returns></returns>
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
            
            //Prevent units from jumping over height discontinuities. 防止单位跳高不连续

            if(checkTerrainHeight && neighborCPos.Layer == 0)
            {
                var from = neighborCPos - direction;
                if(Math.Abs(World.Map.Height[neighborCPos] - World.Map.Height[from]) > 1)
                {
                    return Constants.InvalidNode;
                }
            }
            //Directional bonuses for smoother flow!
           if(LaneBias != 0)
            {
                var ux = neighborCPos.X + (InReverse ? 1 : 0) & 1;
                var uy = neighborCPos.Y + (InReverse ? 1 : 0) & 1;

                if ((ux == 0 && direction.Y < 0) || (ux == 1 && direction.Y > 0))
                    cellCost += LaneBias;

                if ((uy == 0 && direction.X < 0) || (uy == 1 && direction.X > 0))
                    cellCost += LaneBias;
            }
            return cellCost;
        }

        public CellInfo this[CPos pos]
        {
            get
            {
                return  (pos.Layer == 0 ? groundInfo:customLayerInfo[pos.Layer].Second)[pos];
            }
            set
            {
                (pos.Layer == 0 ? groundInfo : customLayerInfo[pos.Layer].Second)[pos] = value;
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
using System;
using System.Collections.Generic;
using EW.Primitives;

namespace EW.Mods.Common.Pathfinder
{
    public interface IPathSearch : IDisposable
    {


        /// <summary>
        /// The graph used by the A*
        /// </summary>
        /// <value>The graph.</value>
        IGraph<CellInfo> Graph { get; }


        /// <summary>
        /// Stores the analyzed nodes by the expand function
        /// 通过扩展功能存储分析的节点
        /// </summary>
        /// <value>The considered.</value>
        IEnumerable<Pair<CPos,int>> Considered { get; }

        Player Owner { get; }

        IPathSearch Reverse();

        IPathSearch WithCustomBlocker(Func<CPos, bool> customBlock);

        IPathSearch WithIgnoredActor(Actor a);

        IPathSearch WithHeuristic(Func<CPos, int> h);

        IPathSearch WithCustomCost(Func<CPos, int> w);

        IPathSearch WithoutLaneBias();

        IPathSearch FromPoint(CPos from);


        /// <summary>
        /// Decides whether a location is a target based on its estimate
        /// (An estimate  of 0 means that the location  and the unit's goal are the same.
        /// There could be multiple goals.
        /// 根据估计值确定一个地点是否为目标（估计值为0 意味着地点和单位的目标相同，可能存在多个目标）
        /// </summary>
        /// <returns><c>true</c>, if target was ised, <c>false</c> otherwise.</returns>
        /// <param name="location">Location.</param>
        bool IsTarget(CPos location);

        bool CanExpand { get; }

        CPos Expand();
    }



    public abstract class BasePathSearch:IPathSearch
    {
        public IGraph<CellInfo> Graph { get; set; }

        public abstract IEnumerable<Pair<CPos,int>> Considered { get; }

        protected IPriorityQueue<GraphConnection> OpenQueue { get; private set; }

        /// <summary>
        /// This member is used to compute the Id of PathSearch
        /// Essentially,it represents a collection of initial points considered and their Heuristics to reach the target.
        /// It pretty match identifies,in conjunction(结合) of the Actor,
        /// a deterministic set of calculations.
        /// 该成员用于计算PathSearch 的ID，本质上，它代表了所考虑的初始点的集合以及它们的启发式来达到目标
        /// </summary>
        protected readonly IPriorityQueue<GraphConnection> StartPoints;

        public Player Owner { get { return Graph.Actor.Owner; } }

        public int MaxCost { get; private set; }

        public bool Debug { get; set; }

        protected Func<CPos, int> heuristic;    //启发式，下探
        protected Func<CPos, bool> isGoal;


        protected BasePathSearch(IGraph<CellInfo> graph)
        {
            Graph = graph;
            OpenQueue = new PriorityQueue<GraphConnection>(GraphConnection.ConnectionCostComparer);
            StartPoints = new PriorityQueue<GraphConnection>(GraphConnection.ConnectionCostComparer);
        }


        public IPathSearch Reverse()
        {
            Graph.InReverse = true;
            return this;
        }

        public IPathSearch WithCustomBlocker(Func<CPos,bool> customBlock)
        {
            Graph.CustomBlock = customBlock;
            return this;
        }

        public IPathSearch WithIgnoredActor(Actor a)
        {
            Graph.IgnoreActor = a;
            return this;
        }

        public IPathSearch WithHeuristic(Func<CPos,int> h)
        {
            heuristic = h;
            return this;
        }

        public IPathSearch WithCustomCost(Func<CPos,int> f)
        {
            Graph.CustomCost = f;
            return this;
        }

        public IPathSearch WithoutLaneBias()
        {
            Graph.LaneBias = 0;
            return this;
        }

        public IPathSearch FromPoint(CPos from)
        {
            if (Graph.World.Map.Contains(from))
                AddInitialCell(from);
            return this;
        }

        public bool IsTarget(CPos location)
        {
            return isGoal(location);
        }

        public abstract CPos Expand();

        public bool CanExpand { get { return !OpenQueue.Empty; } }





        /// <summary>
        /// Default:Diagonal distance heuristic
        /// 预估委托
        /// </summary>
        /// <param name="destination"></param>
        /// <returns> a gelegate that calcuates the estimation for a node</returns>
        protected static Func<CPos,int> DefaultEstimator(CPos destination)
        {
            return here =>
            {
                var diag = Math.Min(Math.Abs(here.X - destination.X), Math.Abs(here.Y - destination.Y));

                var straight = Math.Abs(here.X - destination.X) + Math.Abs(here.Y - destination.Y);

                return Constants.CellCost * straight + (Constants.DiagonalCellCost - 2 * Constants.CellCost) * diag;
            };
        }


        protected abstract void AddInitialCell(CPos cell);


        public void Dispose() {
        
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing){

            if (disposing)
                Graph.Dispose();
        }
    }
}
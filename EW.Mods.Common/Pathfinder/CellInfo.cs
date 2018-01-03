using System;
namespace EW.Mods.Common.Pathfinder
{
    /// <summary>
    /// 
    /// </summary>
    public enum CellStatus
    {
        Unvisited,
        Open,
        Closed,
    }
    public struct CellInfo
    {
        /// <summary>
        /// The cost to move from the start up to this node
        /// 从启动到此节点的消耗费用
        /// </summary>
        public readonly int CostSoFar;

        /// <summary>
        /// The estimation of how far is the node from our goal
        /// 估计节点距离我们的目标有多远
        /// </summary>
        public readonly int EstimatedTotal;

        /// <summary>
        /// The previous node of this one that follows the shortest path.
        /// 跟随最短路径的前一个节点
        /// </summary>
        public readonly CPos PreviousPos;

        /// <summary>
        /// The status of this node.
        /// </summary>
        public readonly CellStatus Status;

        public CellInfo(int costSoFar,int estimatedTotal,CPos previousPos,CellStatus status)
        {
            this.CostSoFar = costSoFar;
            this.EstimatedTotal = estimatedTotal;
            this.PreviousPos = previousPos;
            this.Status = status;
        }
    }
}
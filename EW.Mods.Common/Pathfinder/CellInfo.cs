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
        /// 从启动到此节点的消耗费用
        /// </summary>
        public readonly int CostSoFar;

        /// <summary>
        /// 估计节点距离我们的目标有多远
        /// </summary>
        public readonly int EstimatedTotal;

        /// <summary>
        /// 跟随最短路径的前一个节点
        /// </summary>
        public readonly CPos PreviousPos;

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
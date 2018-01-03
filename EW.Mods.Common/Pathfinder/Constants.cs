using System;


namespace EW.Mods.Common.Pathfinder
{
    public static class Constants
    {
        /// <summary>
        /// Min cost to arrive from once cell to an adjacent one
        /// (125 according to runtime tests where we could assess the cost a unit took to move one cell horizontally)
        /// 根据运行时测试，我们可以评估一个单位水平移动一个单元格的成本
        /// </summary>
        public const int CellCost = 125;

        /// <summary>
        /// 到达对角线相邻单元格最小成本
        /// Min cost to arrive from once cell to a diagonal adjacent one 
        /// (125 * Sqrt(2) according to runtime tests where we could assess the cost a unit took to move one cell diagonally)
        /// </summary>
        public const int DiagonalCellCost = 177;

        public const int InvalidNode = int.MaxValue;
    }
}
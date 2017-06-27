using System;
using System.Collections.Generic;
using EW.Xna.Platforms;
namespace EW.Primitives
{
    /// <summary>
    /// 空间划分
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SpatiallyPartitioned<T>
    {
        readonly int rows, cols, binSize;

        readonly Dictionary<T, Rectangle>[] itemBoundsBins;
        public SpatiallyPartitioned(int width,int height,int binSize)
        {
            this.binSize = binSize;

            rows = Exts.IntegerDivisionRoundingAwayFromZero(height, binSize);
            cols = Exts.IntegerDivisionRoundingAwayFromZero(width, binSize);
            itemBoundsBins = Exts.MakeArray(rows * cols, _ => new Dictionary<T, Rectangle>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="minRow"></param>
        /// <param name="maxRow"></param>
        /// <param name="minCol"></param>
        /// <param name="maxCol"></param>
        void BoundsToBinRowsAndCols(Rectangle bounds,out int minRow,out int maxRow,out int minCol,out int maxCol)
        {
            var top = Math.Min(bounds.Top, bounds.Bottom);
            var bottom = Math.Max(bounds.Top, bounds.Bottom);
            var left = Math.Min(bounds.Left, bounds.Right);
            var right = Math.Max(bounds.Left, bounds.Right);

            minRow = Math.Max(0, top / binSize);
            minCol = Math.Max(0, left / binSize);
            maxRow = Math.Min(rows, Exts.IntegerDivisionRoundingAwayFromZero(bottom, binSize));
            maxCol = Math.Min(cols, Exts.IntegerDivisionRoundingAwayFromZero(right, binSize));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public IEnumerable<T> InBox(Rectangle box)
        {
            int minRow, maxRow, minCol, maxCol;
            BoundsToBinRowsAndCols(box, out minRow, out maxRow, out minCol, out maxCol);

            var items = minRow >= maxRow || minCol >= maxCol ? null : new HashSet<T>();

            for(var row = minRow; row < maxRow; row++)
            {
                for(var col = minCol; col < maxCol; col++)
                {
                    var binBounds = BinBounds(row, col);
                    foreach(var kvp in BinAt(row, col))
                    {
                        var item = kvp.Key;
                        var bounds = kvp.Value;

                        if (bounds.Intersects(box) && (items == null || binBounds.Contains(bounds) || items.Add(item)))
                            yield return item;
                            

                    }
                }
            }
        }

        Rectangle BinBounds(int row,int col)
        {
            return new Rectangle(col * binSize, row * binSize, binSize, binSize);
        }

        Dictionary<T,Rectangle> BinAt(int row,int col)
        {
            return itemBoundsBins[row * cols + col];
        }
    }
}
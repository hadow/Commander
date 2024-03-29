﻿using System;
using System.Collections.Generic;
using EW.Framework;
using System.Drawing;
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

        readonly Dictionary<T, Rectangle> itemBounds = new Dictionary<T, Rectangle>();

        readonly Action<Dictionary<T, Rectangle>, T, Rectangle> addItem = (bin, actor, bounds) => bin.Add(actor, bounds);
        readonly Action<Dictionary<T, Rectangle>, T, Rectangle> removeItem = (bin, actor, bounds) => bin.Remove(actor);
        public SpatiallyPartitioned(int width,int height,int binSize)
        {
            this.binSize = binSize;

            rows = Exts.IntegerDivisionRoundingAwayFromZero(height, binSize);
            cols = Exts.IntegerDivisionRoundingAwayFromZero(width, binSize);
            itemBoundsBins = Exts.MakeArray(rows * cols, _ => new Dictionary<T, Rectangle>());
        }

        void ValidateBounds(T actor,Rectangle bounds)
        {
            if (bounds.Width == 0 || bounds.Height == 0)
                throw new ArgumentException("Bounds of actor {0} are empty.".F(actor));
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
        /// <param name="actor"></param>
        /// <param name="bounds"></param>
        /// <param name="action"></param>
        void MutateBins(T actor,Rectangle bounds,Action<Dictionary<T,Rectangle>,T,Rectangle> action)
        {
            int minRow, maxRow, minCol, maxCol;
            BoundsToBinRowsAndCols(bounds, out minRow, out maxRow, out minCol, out maxCol);

            for(var row = minRow; row < maxRow; row++)
            {
                for (var col = minCol; col < maxCol; col++)
                    action(BinAt(row, col), actor, bounds);
            }
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


            //We want to return any items intersecting the box
            //If the box covers multiple bins,we must handle items that are contained in multiple bins and avoid returning them more than once
            //We shall use a set to track these.
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


                        //If the item is in the bin,we must check intersects the box before returning it.
                        //We shall track it in the set of items seen so far to avoid returning it again if it appears in another bin.
                        //PERF: If the item is wholly contained within the bin,we can avoid the cost of tracking it.
                        if (bounds.IntersectsWith(box) && (items == null || binBounds.Contains(bounds) || items.Add(item)))
                            yield return item;
                            

                    }
                }
            }
        }

        /// <summary>
        /// Bins the bounds.
        /// </summary>
        /// <returns>The bounds.</returns>
        /// <param name="row">Row.</param>
        /// <param name="col">Col.</param>
        Rectangle BinBounds(int row,int col)
        {
            return new Rectangle(col * binSize, row * binSize, binSize, binSize);
        }

        Dictionary<T,Rectangle> BinAt(int row,int col)
        {
            return itemBoundsBins[row * cols + col];
        }

        public void Add(T item,Rectangle bounds)
        {
            ValidateBounds(item, bounds);
            itemBounds.Add(item, bounds);
            MutateBins(item, bounds, addItem);
        }

        //public void Remove(T item)
        //{
        //    MutateBins(item, itemBounds[item], removeItem);
        //    itemBounds.Remove(item);
        //}

        public void Update(T item,Rectangle bounds)
        {
            ValidateBounds(item, bounds);
            MutateBins(item, itemBounds[item], removeItem);
            MutateBins(item, itemBounds[item] = bounds, addItem);
        }


        public bool Remove(T item)
        {
            Rectangle bounds;
            if (!itemBounds.TryGetValue(item, out bounds))
                return false;

            MutateBins(item, bounds, removeItem);
            itemBounds.Remove(item);
            return true;
        }


        public IEnumerable<T> At(Int2 location){

            var col = (location.X / binSize).Clamp(0, cols - 1);
            var row = (location.Y / binSize).Clamp(0, rows - 1);

            foreach(var kvp in BinAt(row,col)){
                if (kvp.Value.Contains(location))
                    yield return kvp.Key;
            }
        }


        public IEnumerable<Rectangle> ItemBounds{
            get{
                return itemBounds.Values;
            }
        }


        public bool Contains(T item)
        {
            return itemBounds.ContainsKey(item);
        }
    }
}
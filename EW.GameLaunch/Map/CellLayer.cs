using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
namespace EW
{
    /// <summary>
    /// 覆盖地图的“something"层
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CellLayer<T>:IEnumerable<T>
    {
        public readonly Size Size;
        readonly Rectangle bounds;
        /// <summary>
        /// 地图网格类型
        /// </summary>
        public readonly MapGridT GridT;
        public event Action<CPos> CellEntryChanged = null;
        readonly T[] entries;
        public CellLayer(Map map):this(map.Grid.Type,new Size((int)map.MapSize.X, (int)map.MapSize.Y))
        {

        }
        public CellLayer(MapGridT gridT,Size size)
        {
            Size = size;
            bounds = new Rectangle(0, 0, Size.Width, Size.Height);
            GridT = gridT;
            entries = new T[size.Width * size.Height];
        }


        public void CopyValuesFrom(CellLayer<T> anotherLayer)
        {
            if (Size != anotherLayer.Size || GridT != anotherLayer.GridT)
                throw new ArgumentException("layers must have a matching size and shape (grid type).", "anotherlayer");

            if (CellEntryChanged != null)
                throw new InvalidOperationException("Cannot copy values when there are listeners attached to the CellEntryChanged event.");

            Array.Copy(anotherLayer.entries, entries, entries.Length);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)entries).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(CPos cell)
        {
            if (GridT == MapGridT.RectangularIsometric && cell.X < cell.Y)
                return false;
            return Contains(cell.ToMPos(GridT));
        }

        public bool Contains(MPos uv)
        {
            return bounds.Contains(uv.U, uv.V);
        }


        /// <summary>
        /// Resolve an array index from map coordinates
        /// </summary>
        /// <param name="uv"></param>
        /// <returns></returns>
        int Index(MPos uv)
        {
            return uv.V * Size.Width + uv.U;
        }

        /// <summary>
        /// Resolve an array index from cell coordinates
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        int Index(CPos cell)
        {
            return Index(cell.ToMPos(GridT));
        }
            

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uv"></param>
        /// <returns></returns>
        public T this[MPos uv]
        {
            get { return entries[Index(uv)]; }
            set
            {
                entries[Index(uv)] = value;

                if (CellEntryChanged != null)
                    CellEntryChanged(uv.ToCPos(GridT));
            }
        }
        /// <summary>
        /// Resolve an array index from cell coordinates.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public T this[CPos cell]
        {
            get
            {
                return entries[Index(cell)];
            }
            set
            {
                entries[Index(cell)] = value;

                if (CellEntryChanged != null)
                    CellEntryChanged(cell);
            }
        }

        public MPos Clamp(MPos uv)
        {
            return uv.Clamp(new Rectangle(0, 0, Size.Width - 1, Size.Height - 1));
        }

        /// <summary>
        /// Clears the layer contents with a known value.
        /// </summary>
        /// <param name="clearValue"></param>
        public void Clear(T clearValue)
        {
            for (var i = 0; i < entries.Length; i++)
                entries[i] = clearValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialCellValueFactory"></param>
        /// <param name="size"></param>
        /// <param name="mapGridT"></param>
        /// <returns></returns>
        public static CellLayer<T> CreateInstance(Func<MPos,T> initialCellValueFactory,Size size,MapGridT mapGridT)
        {
            var cellLayer = new CellLayer<T>(mapGridT, size);
            for(var v = 0; v < size.Height; v++)
            {
                for(var u = 0; u < size.Width; u++)
                {
                    var mpos = new MPos(u, v);
                    cellLayer[mpos] = initialCellValueFactory(mpos);
                }
            }
            return cellLayer;
        }
            
    }
}
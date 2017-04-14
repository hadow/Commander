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
        readonly EW.Xna.Platforms.Rectangle bounds;
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
            bounds = new EW.Xna.Platforms.Rectangle(0, 0, Size.Width, Size.Height);
            GridT = gridT;
            entries = new T[size.Width * size.Height];
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

        

        int Index(MPos uv)
        {
            return uv.V * Size.Width + uv.U;
        }
            

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

        public MPos Clamp(MPos uv)
        {
            return uv.Clamp(new Xna.Platforms.Rectangle(0, 0, Size.Width - 1, Size.Height - 1));
        }
    }
}
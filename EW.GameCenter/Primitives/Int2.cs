using System;
namespace EW.Primitives
{
    public struct Int2:IEquatable<Int2>
    {
        public readonly int X, Y;

        public Int2(int x,int y) { X = x; Y = y; }


        #region Operator

        public static Int2 operator +(Int2 a,Int2 b) { return new Int2(a.X + b.X, a.Y + b.Y); }

        public static Int2 operator -(Int2 a,Int2 b) { return new Int2(a.X - b.X, a.Y - b.Y); }


        #endregion
    }
}
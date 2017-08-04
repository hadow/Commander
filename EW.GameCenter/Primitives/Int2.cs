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

        public static bool operator ==(Int2 a,Int2 b) { return a.X == b.X && a.Y == b.Y; }

        public static bool operator !=(Int2 a ,Int2 b) { return !(a == b); }
        #endregion

        public bool Equals(Int2 other) { return this == other; }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (obj is Int2) && Equals((Int2)obj);
        }
    }
}
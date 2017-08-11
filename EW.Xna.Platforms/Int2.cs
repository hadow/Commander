using System;
namespace EW.Xna.Platforms
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

        public static Int2 operator /(Int2 a,int b) { return new Int2(a.X / b, a.Y / b); }
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

        public Int2 Clamp(Rectangle r)
        {
            return new Int2(Math.Min(r.Right, Math.Max(X, r.Left)), Math.Min(r.Bottom, Math.Max(Y, r.Top)));
        }

        public Vector2 ToVector2() { return new Vector2(X, Y); }
    }
}
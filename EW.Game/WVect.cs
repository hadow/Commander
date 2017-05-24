using System;

namespace EW
{
    public struct WVect
    {
        public readonly int X, Y, Z;
        
        public WVect(int x,int y,int z) { X = x;Y = y; Z = z; }

        public WVect(WDist x,WDist y,WDist z)
        {
            X = x.Length;
            Y = y.Length;
            Z = z.Length;
        }

        #region Operator

        public static WVect operator +(WVect a,WVect b) { return new WVect(a.X + b.X, a.Y + b.Y, a.Z + b.Z); }
        public static WVect operator -(WVect a,WVect b) { return new WVect(a.X - b.X, a.Y - b.Y, a.Z - b.Z); }

        public static WVect operator -(WVect a) { return new WVect(-a.X, -a.Y, -a.Z); }

        #endregion
    }
}
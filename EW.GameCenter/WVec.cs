using System;

namespace EW
{
    public struct WVec
    {
        public readonly int X, Y, Z;
        
        public WVec(int x,int y,int z) { X = x;Y = y; Z = z; }

        public WVec(WDist x,WDist y,WDist z)
        {
            X = x.Length;
            Y = y.Length;
            Z = z.Length;
        }

        public static readonly WVec Zero = new WVec(0, 0, 0);

        #region Operator

        public static WVec operator +(WVec a,WVec b) { return new WVec(a.X + b.X, a.Y + b.Y, a.Z + b.Z); }
        public static WVec operator -(WVec a,WVec b) { return new WVec(a.X - b.X, a.Y - b.Y, a.Z - b.Z); }

        public static WVec operator -(WVec a) { return new WVec(-a.X, -a.Y, -a.Z); }

        #endregion
    }
}
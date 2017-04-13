using System;


namespace EW
{
    public struct WVector
    {
        public readonly int X, Y, Z;
        
        public WVector(int x,int y,int z) { X = x;Y = y; Z = z; }

        public WVector(WDist x,WDist y,WDist z)
        {
            X = x.Length;
            Y = y.Length;
            Z = z.Length;
        }
    }
}
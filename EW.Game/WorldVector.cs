using System;


namespace EW
{
    public struct WorldVector
    {
        public readonly int X, Y, Z;
        
        public WorldVector(int x,int y,int z) { X = x;Y = y; Z = z; }

        public WorldVector(WorldDist x,WorldDist y,WorldDist z)
        {
            X = x.Length;
            Y = y.Length;
            Z = z.Length;
        }
    }
}
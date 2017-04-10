using System;

namespace EW
{
    public struct WorldPos:IEquatable<WorldPos>
    {
        public readonly int X, Y, Z;

        public WorldPos(int x,int y,int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public bool Equals(WorldPos other)
        {
            return other == this;
        }

        #region operator

        public static bool operator ==(WorldPos a,WorldPos b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator !=(WorldPos a,WorldPos b)
        {
            return !(a == b);
        }

        public static WorldPos operator +(WorldPos a,WorldVector b)
        {
            return new WorldPos(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static WorldPos operator -(WorldPos a,WorldVector b)
        {
            return new WorldPos(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
        #endregion
    }
}
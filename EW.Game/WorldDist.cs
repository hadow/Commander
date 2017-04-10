using System;


namespace EW
{
    /// <summary>
    /// 
    /// </summary>
    public struct WorldDist:IEquatable<WorldDist>,IComparable,IComparable<WorldDist>
    {
        public readonly int Length;

        public int CompareTo(object obj)
        {
            if (!(obj is WorldDist))
                return 1;
            return Length.CompareTo(((WorldDist)obj).Length);
        }

        public int CompareTo(WorldDist other)
        {
            return Length.CompareTo(other.Length);
        }

        public bool Equals(WorldDist other)
        {
            return other == this;
        }

        #region Operator

        public static bool operator ==(WorldDist a,WorldDist b)
        {
            return a.Length == b.Length;
        }

        public static bool operator !=(WorldDist a,WorldDist b)
        {
            return !(a == b);
        }

        #endregion


    }
}
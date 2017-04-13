using System;


namespace EW
{
    /// <summary>
    /// 
    /// </summary>
    public struct WDist:IEquatable<WDist>,IComparable,IComparable<WDist>
    {
        public static readonly WDist Zero = new WDist(0);
        public readonly int Length;

        public WDist(int r) { Length = r; }
        public int CompareTo(object obj)
        {
            if (!(obj is WDist))
                return 1;
            return Length.CompareTo(((WDist)obj).Length);
        }

        public int CompareTo(WDist other)
        {
            return Length.CompareTo(other.Length);
        }

        public bool Equals(WDist other)
        {
            return other == this;
        }

        #region Operator

        public static bool operator ==(WDist a,WDist b)
        {
            return a.Length == b.Length;
        }

        public static bool operator !=(WDist a,WDist b)
        {
            return !(a == b);
        }

        #endregion


    }
}
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

        public long LengthSquared { get { return (long)Length * Length; } }

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

        public static bool TryParse(string s,out WDist result)
        {
            result = Zero;
            if (string.IsNullOrEmpty(s))
                return false;
            s = s.ToLowerInvariant();

            var components = s.Split('c');
            var cell = 0;
            var subCell = 0;
            switch (components.Length)
            {
                case 2:
                    if (!Exts.TryParseIntegerInvariant(components[0], out cell) || !Exts.TryParseIntegerInvariant(components[1], out subCell))
                        return false;
                    break;
                case 1:
                    if (!Exts.TryParseIntegerInvariant(components[0], out subCell))
                        return false;
                    break;
                default:
                    return false;
            }

            if (cell < 0)
                subCell = -subCell;

            result = new WDist(1024 * cell + subCell);
            return true;
        }

    }
}
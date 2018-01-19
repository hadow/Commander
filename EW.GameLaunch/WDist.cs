using System;
using System.Linq;
using Eluant;
using Eluant.ObjectBinding;
using EW.Scripting;
using EW.Support;

namespace EW
{
    /// <summary>
    /// 1d world distance - 1024 units = 1 cell
    /// </summary>
    public struct WDist:IEquatable<WDist>,IComparable,IComparable<WDist>,IScriptBindable,ILuaAdditionBinding,ILuaSubtractionBinding,ILuaEqualityBinding,ILuaTableBinding
    {
        public static readonly WDist Zero = new WDist(0);
        public static readonly WDist MaxValue = new WDist(int.MaxValue);
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

        public override int GetHashCode()
        {
            return Length.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is WDist && Equals((WDist)obj);
        }

        #region Operator

        public static WDist operator +(WDist a,WDist b) { return new WDist(a.Length + b.Length); }

        public static WDist operator -(WDist a,WDist b) { return new WDist(a.Length - b.Length); }

        public static bool operator ==(WDist a,WDist b)
        {
            return a.Length == b.Length;
        }

        public static bool operator !=(WDist a,WDist b)
        {
            return !(a == b);
        }
        public static bool operator <(WDist a,WDist b) { return a.Length < b.Length; }

        public static bool operator >(WDist a,WDist b) { return a.Length > b.Length; }

        public static bool operator >=(WDist a,WDist b) { return a.Length >= b.Length; }

        public static bool operator <=(WDist a,WDist b) { return a.Length <= b.Length; }

        public static WDist operator -(WDist a) { return new WDist(-a.Length); }

        public static WDist operator /(WDist a, int b) { return new WDist(a.Length / b); }


        public static WDist operator *(int a,WDist b) { return new WDist(a * b.Length); }
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
                    if (!Exts.TryParseIntegerInvariant(components[0], out cell) ||
                        !Exts.TryParseIntegerInvariant(components[1], out subCell))
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

        public static WDist FromCells(int cells) { return new WDist(1024 * cells); }

        //Sampled a N-sample probability density function in the range[-1024...1024]
        //1 sample produces a rectangular probability
        //2 sample produces a triangular probability.
        //
        //N samples approximates a true Gaussian.
        public static WDist FromPDF(MersenneTwister r,int samples)
        {
            return new WDist(Exts.MakeArray(samples, _ => r.Next(-1024, 1024)).Sum() / samples);
        }
        #region Scripting interface

        public LuaValue Add(LuaRuntime runtime,LuaValue left,LuaValue right)
        {
            WDist a;
            WDist b;
            if (!left.TryGetClrValue(out a) || !right.TryGetClrValue(out b))
                throw new LuaException("Attempted to call WDist.Add(WDist,WDist) with invalid arguments.");

            return new LuaCustomClrObject(a + b);
        }

        public LuaValue Subtract(LuaRuntime runtime,LuaValue left,LuaValue right)
        {
            WDist a;
            WDist b;
            if (!left.TryGetClrValue(out a) || !right.TryGetClrValue(out b))
                throw new LuaException("Attempted to call WDist.Subtract(WDist,WDist) with invalid arguments.");

            return new LuaCustomClrObject(a - b);
        }


        public LuaValue Equals(LuaRuntime runtime,LuaValue left,LuaValue right)
        {
            WDist a;
            WDist b;
            if (!left.TryGetClrValue(out a) || !right.TryGetClrValue(out b))
                throw new LuaException("Attempted to call WDist.Equals(WDist,WDist) with invalid arguments.");

            return a == b;
        }


        public LuaValue this[LuaRuntime runtime,LuaValue key]
        {
            get
            {
                switch (key.ToString())
                {
                    case "Length":return Length;
                    case "Range":return Length;
                    default:
                        throw new LuaException("WDist does not defina a member '{0}'".F(key));
                }
            }

            set
            {
                throw new LuaException("WDist is read-only.Use WDist.New to create a new value");
            }
        }


        #endregion
    }
}
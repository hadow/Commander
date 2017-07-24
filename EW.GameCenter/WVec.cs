using System;
using Eluant;
using Eluant.ObjectBinding;
using EW.Scripting;
namespace EW
{
    public struct WVec:ILuaAdditionBinding,ILuaSubtractionBinding,ILuaUnaryMinusBinding,ILuaEqualityBinding,ILuaTableBinding,IEquatable<WVec>
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

        public static bool operator ==(WVec a,WVec b) { return a.X == b.X && a.Y == b.Y && a.Z == b.Z; }

        public static bool operator !=(WVec a,WVec b) { return !(a == b); }
        #endregion

        public int Length { get { return (int)Exts.ISqrt(LengthSquared); } }

        public long LengthSquared { get { return (long)X * X + (long)Y * Y + (long)Z * Z; } }

        public long HorizontalLengthSquared { get { return (long)X * X + (long)Y * Y; } }

        public bool Equals(WVec other) { return other == this; }

        public override bool Equals(object obj)
        {
            return obj is WVec && Equals((WVec)obj);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }
        public WAngle Yaw
        {
            get
            {
                if (LengthSquared == 0)
                    return WAngle.Zero;

                return WAngle.Zero;
            }
        }

        #region Scripting interface

        public LuaValue Add(LuaRuntime runtime, LuaValue left, LuaValue right)
        {
            WVec a, b;
            if (!left.TryGetClrValue(out a) || !right.TryGetClrValue(out b))
            {
                throw new LuaException("Attempted to call WVec.Add(WVec,WVec) with invalid arguments ({0},{1})".F(left.WrappedClrType().Name, right.WrappedClrType().Name));
            }
            return new LuaCustomClrObject(a + b);
        }

        public LuaValue Subtract(LuaRuntime runtime,LuaValue left,LuaValue right)
        {
            WVec a, b;
            if (!left.TryGetClrValue(out a) || !right.TryGetClrValue(out b))
                throw new LuaException("Attempted to call WVect.Subtract(WVec,WVec) with invalid arguments ({0},{1})".F(left.WrappedClrType().Name, right.WrappedClrType().Name));
            return new LuaCustomClrObject(a - b);
        }

        public LuaValue Minus(LuaRuntime runtime)
        {
            return new LuaCustomClrObject(-this);
        }

        public LuaValue Equals(LuaRuntime runtime,LuaValue left,LuaValue right)
        {
            WVec a, b;
            if (!left.TryGetClrValue(out a) || !right.TryGetClrValue(out b))
                return false;
            return a == b;
        }

        public LuaValue this[LuaRuntime runtime,LuaValue key]
        {
            get
            {
                switch (key.ToString())
                {
                    case "X":return X;
                    case "Y":return Y;
                    case "Z":return Z;
                    case "Facing":return Yaw.Facing;
                    default:
                        throw new LuaException("WVec does not define a member '{0}'".F(key));
                }
            }
            set
            {
                throw new LuaException("WVec is read-only,Use WVec.New to create a new value");
            }
        }
        #endregion
    }
}
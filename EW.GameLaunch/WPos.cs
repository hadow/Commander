using System;
using System.Collections.Generic;
using System.Linq;
using Eluant;
using Eluant.ObjectBinding;
using EW.Scripting;
using EW.Framework;
namespace EW
{
    /// <summary>
    /// Location in the game world
    /// </summary>
    public struct WPos:IScriptBindable,IEquatable<WPos>,ILuaAdditionBinding,ILuaSubtractionBinding,ILuaEqualityBinding,ILuaTableBinding
    {
        public readonly int X, Y, Z;

        public static readonly WPos Zero = new WPos(0, 0, 0);
        public WPos(int x,int y,int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public WPos(WDist x,WDist y,WDist z)
        {
            X = x.Length;
            Y = y.Length;
            Z = z.Length;
           
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }
        public bool Equals(WPos other)
        {
            return other == this;
        }

        public override bool Equals(object obj)
        {
            return (obj is WPos && Equals((WPos)obj));
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        public override string ToString()
        {
            return X + "," + Y + "," + Z;
        }

        public static WPos Lerp(WPos a,WPos b,int mul,int div)
        {
            return a + (b - a) * mul / div;
        }

        public static WPos Lerp(WPos a,WPos b,long mul,long div)
        {
            // The intermediate variables may need more precision than
            // an int can provide, so we can't use WPos.
            var x = (int)(a.X + (b.X - a.X) * mul / div);
            var y = (int)(a.Y + (b.Y - a.Y) * mul / div);
            var z = (int)(a.Z + (b.Z - a.Z) * mul / div);

            return new WPos(x, y, z);
        }

        public static WPos LerpQuadratic(WPos a,WPos b,WAngle pitch,int mul,int div)
        {
            //Start with a linear lerp between the points.
            var ret = Lerp(a, b, mul, div);

            if (pitch.Angle == 0)
                return ret;

            //Add an additional quadratic variation to height
            //Uses decimal to avoid integer overflow
            //添加一个额外二次方程式变化高度
            //使用十进制来避免整数溢出
            var offset = (decimal)(b - a).Length * pitch.Tan() * mul * (div - mul) / (1024 * div * div);
            var clampedOffset = (int)(offset + (decimal)ret.Z).Clamp<Decimal>((decimal)int.MinValue, (decimal)int.MaxValue);

            return new WPos(ret.X, ret.Y, clampedOffset);
        }

        #region operator

        public static bool operator ==(WPos a,WPos b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator !=(WPos a,WPos b)
        {
            return !(a == b);
        }

        public static WPos operator +(WPos a,WVec b)
        {
            return new WPos(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static WPos operator -(WPos a,WVec b)
        {
            return new WPos(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static  WVec operator -(WPos a,WPos b)
        {
            return new WVec(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
        #endregion

        #region Scripting interface

        public LuaValue Add(LuaRuntime runtime,LuaValue left,LuaValue right)
        {
            WPos a;
            WVec b;
            if (!left.TryGetClrValue(out a) || !right.TryGetClrValue(out b))
                throw new LuaException("Attempted to call WPos.Add(WPos,WVect) with invalid arguments ({0},{1})".F(left.WrappedClrType().Name, right.WrappedClrType().Name));
            return new LuaCustomClrObject(a + b);
        }

        public LuaValue Subtract(LuaRuntime runtime,LuaValue left,LuaValue right)
        {
            WPos a;
            var rightT = right.WrappedClrType();
            if (!left.TryGetClrValue(out a))
                throw new LuaException("Attempted to call WPos.Subtract(WPos,WVect) with invalid arguments ({0},{1})".F(left.WrappedClrType().Name, rightT));

            if(rightT == typeof(WPos))
            {
                WPos b;
                right.TryGetClrValue(out b);
                return new LuaCustomClrObject(a - b);
            }
            else if(rightT == typeof(WVec))
            {
                WVec b;
                right.TryGetClrValue(out b);
                return new LuaCustomClrObject(a - b);
            }
            throw new LuaException("Attempted to call WPos.Subtract(WPos,WVect) with invalid arguments ({0},{1})".F(left.WrappedClrType().Name, rightT));
        }

        public LuaValue Equals(LuaRuntime runtime,LuaValue left,LuaValue right)
        {
            WPos a, b;
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
                    default:
                        throw new LuaException("WPos does not define a member '{0}'".F(key));
                       
                }
            }
            set
            {
                throw new LuaException("WPos is read-only,Use WPos.New to create a new value");
            }
        }


        #endregion
    }

    public static class IEnumerableExtensions
    {

        public static WPos Average(this IEnumerable<WPos> source)
        {
            var length = source.Count();
            if (length == 0)
                return WPos.Zero;

            var x = 0L;
            var y = 0L;
            var z = 0L;

            foreach(var pos in source)
            {
                x += pos.X;
                y += pos.Y;
                z += pos.Z;
            }

            x /= length;
            y /= length;
            z /= length;

            return new WPos((int)x, (int)y, (int)z);
        }
    }
}
using System;
using Eluant;
using Eluant.ObjectBinding;
using EW.Scripting;
namespace EW
{
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
}
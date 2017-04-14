using System;
using Eluant;
using Eluant.ObjectBinding;
using EW.Scripting;
namespace EW
{
    public struct CVec:IEquatable<CVec>,ILuaAdditionBinding,ILuaSubtractionBinding,ILuaUnaryMinusBinding,ILuaEqualityBinding,ILuaTableBinding
    {

        public readonly int X, Y;
        public static readonly CVec Zero = new CVec(0, 0);
        public CVec(int x,int y) { X = x; Y = y; }

        public bool Equals(CVec other) { return other == this; }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #region Operator

        public static CVec operator +(CVec a ,CVec b)
        {
            return new CVec(a.X + b.X, a.Y + b.Y);
        }

        public static CVec operator -(CVec a,CVec b)
        {
            return new CVec(a.X - b.X, a.Y - b.Y);
        }

        public static CVec operator -(CVec a)
        {
            return new CVec(-a.X, -a.Y);
        }

        public static bool operator ==(CVec a,CVec b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(CVec a,CVec b)
        {
            return !(a == b);
        }


        #endregion


        #region Scripting interface

        public LuaValue Add(LuaRuntime runtime,LuaValue left,LuaValue right)
        {
            CVec a, b;
            if(!left.TryGetClrValue(out a) || !right.TryGetClrValue(out b))
            {
                throw new LuaException("");

            }
            return new LuaCustomClrObject(a + b);
        }

        public LuaValue Subtract(LuaRuntime runtime,LuaValue left,LuaValue right)
        {
            CVec a, b;
            if (!left.TryGetClrValue(out a) || !right.TryGetClrValue(out b))
                throw new LuaException("");

            return new LuaCustomClrObject(a-b);
        }

        public LuaValue Minus(LuaRuntime runtime)
        {
            return new LuaCustomClrObject(-this);
        }

        public LuaValue Equals(LuaRuntime runtime,LuaValue left,LuaValue right)
        {
            CVec a, b;
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
                    case "X":
                        return X;
                    case "Y":
                        return Y;
                    default:
                        throw new LuaException("CVec does not define a memeber '{0}'".F(key));
                }
            }
            set
            {
                throw new LuaException("");
            }
        }

        #endregion

    }
}
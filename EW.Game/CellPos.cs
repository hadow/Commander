using System;
using Eluant;
using Eluant.ObjectBinding;
using EW.Scripting;
using EW.Xna.Platforms;
namespace EW
{
    /// <summary>
    /// 
    /// </summary>
    public struct CellPos:IEquatable<CellPos>,ILuaAdditionBinding,ILuaSubtractionBinding,ILuaEqualityBinding,ILuaTableBinding
    {
        public readonly int X, Y;
        public CellPos(int x,int y) { X = x;Y = y; }

        public static readonly CellPos Zero = new CellPos(0, 0);

        public static explicit operator CellPos(Vector2 a)
        {
            return new CellPos((int)a.X, (int)a.Y);
        }
        public bool Equals(CellPos cPos)
        {
            return X == cPos.X && Y == cPos.Y;
        }
        #region Operator

        public static CellPos operator +(CellPos a,CellVector b)
        {
            return new CellPos(a.X + b.X, a.Y + b.Y);
        }

        public static CellPos operator -(CellPos a,CellVector b)
        {
            return new CellPos(a.X - b.X, a.Y - b.Y);
        }

        public static CellVector operator -(CellPos a,CellPos b)
        {
            return new CellVector(a.X - b.X, a.Y - b.Y);
        }

        public static bool operator ==(CellPos a,CellPos b)
        {
            return a.X == b.X && a.Y == b.Y;
        }
        public static bool operator !=(CellPos a,CellPos b)
        {
            return !(a == b);
        }
        #endregion

        public override bool Equals(object obj)
        {
            return obj is CellPos && Equals((CellPos)obj);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }


        public MPos ToMPos(Map map)
        {
            return ToMPos(map.Grid.Type);
        }

        public MPos ToMPos(MapGridT gridT)
        {
            //TODO:
            return default(MPos);
        }
        #region Scripting Interface
        public LuaValue Add(LuaRuntime runtime,LuaValue left,LuaValue right)
        {
            CellPos a;
            CellVector b;

            if (!left.TryGetClrValue(out a) || !right.TryGetClrValue(out b))
                throw new LuaException("Attempted to call CellPos.Add(CellPos,CellVector) with invalid arguments ({0},{1})");

            return new LuaCustomClrObject(a + b);
        }

        public LuaValue Subtract(LuaRuntime runtime,LuaValue left,LuaValue right)
        {
            CellPos a;
            CellVector b;
            if (!left.TryGetClrValue(out a) || !right.TryGetClrValue(out b))
                throw new LuaException("Attempted to call CellPos.Substrac(CellPos,CellVector) with invalid arguments ({0},{1})");

            return new LuaCustomClrObject(a - b);
        }

        public LuaValue Equals(LuaRuntime runtime,LuaValue left,LuaValue right)
        {
            CellPos a, b;
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
                        throw new LuaException("CellPos does not defina a memeber '{0}'".F(key));
                }
            }
            set
            {
                throw new LuaException("CellPos is read-only,Use CellPos.New to Create a new value");
            }
        }


        #endregion
    }
}
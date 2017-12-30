using System;
using Eluant;
using Eluant.ObjectBinding;
using EW.Scripting;
using EW.OpenGLES;
namespace EW
{
    /// <summary>
    /// 
    /// </summary>
    public struct CPos:IScriptBindable,IEquatable<CPos>,ILuaAdditionBinding,ILuaSubtractionBinding,ILuaEqualityBinding,ILuaTableBinding
    {
        public readonly int X, Y;

        public readonly byte Layer;

        public CPos(int x,int y) { X = x;Y = y; Layer = 0; }

        public CPos(int x,int y,byte layer) { X = x;Y = y;Layer = layer; }

        public static readonly CPos Zero = new CPos(0, 0);

        public static explicit operator CPos(Vector2 a)
        {
            return new CPos((int)a.X, (int)a.Y);
        }
        public bool Equals(CPos cPos)
        {
            return X == cPos.X && Y == cPos.Y;
        }
        #region Operator

        public static CPos operator +(CPos a,CVec b)
        {
            return new CPos(a.X + b.X, a.Y + b.Y);
        }

        public static CPos operator -(CPos a,CVec b)
        {
            return new CPos(a.X - b.X, a.Y - b.Y);
        }

        public static CVec operator -(CPos a,CPos b)
        {
            return new CVec(a.X - b.X, a.Y - b.Y);
        }

        public static bool operator ==(CPos a,CPos b)
        {
            return a.X == b.X && a.Y == b.Y;
        }
        public static bool operator !=(CPos a,CPos b)
        {
            return !(a == b);
        }
        #endregion

        public override bool Equals(object obj)
        {
            return obj is CPos && Equals((CPos)obj);
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
            if (gridT == MapGridT.Rectangular)
                return new MPos(X, Y);


            //Convert from RectangularIsometric cell (x,y) position to rectangular map position (u,v)


            var u = (X - Y) / 2;
            var v = X + Y;
            return new MPos(u,v);
        }
        #region Scripting Interface
        public LuaValue Add(LuaRuntime runtime,LuaValue left,LuaValue right)
        {
            CPos a;
            CVec b;

            if (!left.TryGetClrValue(out a) || !right.TryGetClrValue(out b))
                throw new LuaException("Attempted to call CPos.Add(CPos,CVec) with invalid arguments ({0},{1})");

            return new LuaCustomClrObject(a + b);
        }

        public LuaValue Subtract(LuaRuntime runtime,LuaValue left,LuaValue right)
        {
            CPos a;
            CVec b;
            if (!left.TryGetClrValue(out a) || !right.TryGetClrValue(out b))
                throw new LuaException("Attempted to call CPos.Substrac(CPos,CVec) with invalid arguments ({0},{1})");

            return new LuaCustomClrObject(a - b);
        }

        public LuaValue Equals(LuaRuntime runtime,LuaValue left,LuaValue right)
        {
            CPos a, b;
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
                        throw new LuaException("CPos does not defina a memeber '{0}'".F(key));
                }
            }
            set
            {
                throw new LuaException("CPos is read-only,Use CPos.New to Create a new value");
            }
        }


        #endregion
    }
}
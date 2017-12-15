using System;
using System.Drawing;
namespace EW.OpenGLES
{
    /// <summary>
    /// 2DœÚ¡ø
    /// </summary>
    public struct Vector2:IEquatable<Vector2>
    {
        public float X;
        
        public float Y;

        #region Private Fields
        private static readonly Vector2 zero = new Vector2(0f, 0f);
        private static readonly Vector2 one = new Vector2(1f, 1f);
        #endregion

        public static Vector2 Zero
        {
            get
            {
                return zero;
            }
        }

        public static Vector2 One
        {
            get { return one; }
        }


        public Vector2(Size p)
        {
            X = p.Width;
            Y = p.Height;
        }

        public Vector2(float x,float y)
        {
            this.X = x;
            this.Y = y;
        }

        public Vector2(float value)
        {
            this.X = value;
            this.Y = value;
        }
        public override bool Equals(object obj)
        {
            if(obj is Vector2)
            {
                return Equals((Vector2)obj);
            }
            return false;
        }

        public bool Equals(Vector2 other)
        {
            return (X == other.X && Y == other.Y);
        }

        public override int GetHashCode()
        {
            return (X.GetHashCode()*397) ^ Y.GetHashCode();
        }


        public static Vector2 operator -(Vector2 value)
        {
            value.X = -value.X;
            value.Y = -value.Y;
            return value;
        }

        public static Vector2 operator -(Vector2 val1,Vector2 val2)
        {
            val1.X -= val2.X;
            val1.Y -= val2.Y;
            return val1;
        }

        public static Vector2 operator +(Vector2 val1,Vector2 val2)
        {
            val1.X += val2.X;
            val1.Y += val2.Y;
            return val1;
        }

        public static Vector2 operator *(Vector2 val1,Vector2 val2)
        {
            val1.X *= val2.X;
            val1.Y *= val2.Y;
            return val1;
        }

        public static Vector2 operator *(Vector2 val,float scaleFactor)
        {
            val.X *= scaleFactor;
            val.Y *= scaleFactor;
            return val;
        }

        public static Vector2 operator /(Vector2 val1,Vector2 val2)
        {
            val1.X /= val2.X;
            val1.Y /= val2.Y;
            return val1;
        }

        public static Vector2 operator /(Vector2 val,float divider)
        {
            float factor = 1 / divider;
            val.X *= factor;
            val.Y *= factor;
            return val;
        }

        public static Vector2 operator *(float scaleFactor,Vector2 val)
        {
            val.X *= scaleFactor;
            val.Y *= scaleFactor;
            return val;
        }


        public static bool operator ==(Vector2 val1,Vector2 val2)
        {
            return val1.X == val2.X && val1.Y == val2.Y;
        }

        public static bool operator !=(Vector2 val1,Vector2 val2)
        {
            return val1.X != val2.X || val1.Y != val2.Y;
        }

        public override string ToString()
        {
            return "{X:" + X + " Y:" + Y + "}";
        }

        public Int2 ToInt2()
        {
            return new Int2((int)X, (int)Y);
        }

        public static implicit operator Vector2(Int2 src) { return new Vector2(src.X, src.Y); }


        public static Vector2 Max(Vector2 a,Vector2 b)
        {
            return new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        }

        public static Vector2 Min(Vector2 a,Vector2 b)
        {
            return new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
        }
    }
}
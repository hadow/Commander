using System;
using System.Drawing;
namespace EW.Framework
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


        public Vector2(System.Drawing.Point point){

            this.X = point.X;
            this.Y = point.Y;
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



        public float LengthSquared { get { return X * X + Y * Y; } }

        public float Length
        {
            get
            {
                return (float)Math.Sqrt(LengthSquared);
            }
        }

        public static float Lerp(float a,float b,float t) { return a + t * (b - a); }

        public static Vector2 Lerp(Vector2 a,Vector2 b,float t)
        {
            return new Vector2(Lerp(a.X, b.X, t), Lerp(a.Y, b.Y, t));
        }

        public static Vector2 Lerp(Vector2 a,Vector2 b,Vector2 t)
        {
            return new Vector2(Lerp(a.X, b.X, t.X), Lerp(a.Y, b.Y, t.Y));
        }

        static float Constrain(float x,float a,float b)
        {
            return x < a ? a : x >b ? b  : x;
        }

        public Vector2 Constrain(Vector2 min,Vector2 max)
        {
            return new Vector2(Constrain(X, min.X, max.X), Constrain(Y, min.Y, max.Y));
        }


        public void Normalize()
        {
            float val = 1.0f / (float)Math.Sqrt((X * X) + (Y * Y));
            X *= val;
            Y *= val;
        }

        public static Vector2 FromAngle(float a) { return new Vector2((float)Math.Sin(a), (float)Math.Cos(a)); }

        public static float Distance(Vector2 v1, Vector2 v2){

            float x = v1.X - v2.X, y = v1.Y - v2.Y;

            return (float)Math.Sqrt((x * x) + (y * y));
        }
    }
}
using System;
using System.Runtime.Serialization;
using System.Diagnostics;
namespace RA.Mobile.Platforms
{
    /// <summary>
    /// 2D Point In 2D Space
    /// </summary>
    public struct Point:IEquatable<Point>
    {
        
        private static readonly Point _zeroPoint = new Point();
      
        public static Point Zero
        {
            get { return _zeroPoint; }
        }
        public int X;
        
        public int Y;

        public Point(int value)
        {
            this.X = value;
            this.Y = value;
        }

        public Point(int x,int y)
        {
            this.X = x;
            this.Y = y;
        }



        public static Point operator +(Point value1,Point value2)
        {
            return new Point(value1.X + value2.X, value1.Y + value2.Y);
        }

        public static Point operator -(Point value1,Point value2)
        {
            return new Point(value1.X - value2.X, value1.Y - value2.Y);
        }

        public static Point operator *(Point value1,Point value2)
        {
            return new Platforms.Point(value1.X * value2.X, value1.Y * value2.Y);
        }


        public static Point operator /(Point value1,Point value2)
        {
            return new Point(value1.X / value2.X, value1.Y / value2.Y);
        }

        public static bool operator ==(Point a,Point b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(Point a,Point b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object other)
        {
            return (other is Point) && Equals((Point)other);
        }


        public bool Equals(Point other)
        {
            return ((X == other.X) && (Y == other.Y));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                return hash;
            }
        }

        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }


    }
}
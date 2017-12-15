using System;

namespace EW.OpenGLES
{

    /// <summary>
    /// 2D ¾ØÐÎ
    /// </summary>
    public struct Rectangle : IEquatable<Rectangle>
    {

        private static Rectangle emptyRectangle = new Rectangle();
        public int X;
        public int Y;

        public int Width;
        public int Height;


        public Rectangle(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }
        public static bool operator ==(Rectangle a, Rectangle b)
        {
            return a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height;
        }
        public static bool operator !=(Rectangle a, Rectangle b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(Rectangle other)
        {
            return false;
        }

        public bool Contains(Rectangle value)
        {
            return ((this.X <= value.X) && (value.X + value.Width) <= (this.X + this.Width)) && ((this.Y <= value.Y) && (value.Y + value.Height) <= (this.Y + this.Height));
        }

        public bool Contains(int x, int y)
        {
            return (((this.X <= x) && (x < (this.X + this.Width))) && ((this.Y <= y) && (y < (this.Y + this.Height))));
        }

        public int Right
        {
            get { return (this.X + this.Width); }
        }

        public int Left
        { get { return this.X; } }

        public int Top
        {
            get { return this.Y; }
        }

        public int Bottom
        {
            get { return this.Y + this.Height; }
        }

        public static Rectangle Empty
        {
            get { return emptyRectangle; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lef"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        /// <returns></returns>
        public static Rectangle FromLTRB(int left, int top, int right, int bottom)
        {
            return new Rectangle(left, top, right - left, bottom - top);
        }

        public bool Intersects(Rectangle val)
        {
            return val.Left < Right &&
                    Left < val.Right &&
                    val.Top < Bottom &&
                    Top < val.Bottom;
        }

        public void Offset(int offsetX, int offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }


        public Point Size
        {
            get { return new Point(this.Width, this.Height); }
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }
    }
}
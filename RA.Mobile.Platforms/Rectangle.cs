using System;


namespace RA.Mobile.Platforms
{
    /// <summary>
    /// 2D ¾ØÐÎ
    /// </summary>
    public struct Rectangle:IEquatable<Rectangle>
    {

        public int X;
        public int Y;

        public int Width;
        public int Height;


        public Rectangle(int x,int y ,int width,int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }
        public static bool operator ==(Rectangle a,Rectangle b)
        {
            return a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height;
        }
        public static bool operator !=(Rectangle a,Rectangle b)
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


    }
}
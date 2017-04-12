using System;
using System.Runtime.Serialization;
namespace EW.Xna.Platforms.Graphics
{
    /// <summary>
    /// 视窗口
    /// </summary>
    /// 
    [DataContract]
    public struct Viewport
    {

        private int x;
        public int X
        {
            get { return x; }
            set { x = value; }
        }

        private int y;
        public int Y
        {
            get { return y; }
            set { y = value; }
        }
        private int width;
        public int Width
        {
            get { return width; }
            set { width = value; }
        }
        private int height;
        public int Height
        {
            get { return height; }
            set { height = value; }
        }
        private float minDepth;

        public float MinDepth
        {
            get { return this.minDepth; }
            set { minDepth = value; }
        }

        private float maxDepth;
        public float MaxDepth
        {
            get { return this.maxDepth; }
            set { maxDepth = value; }
        }

        public Viewport(int x,int y,int width,int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.minDepth = 0.0f;
            this.maxDepth = 1.0f;
        }

        public Viewport(int x,int y,int width,int height,float minDepth,float maxDepth)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.minDepth = minDepth;
            this.maxDepth = maxDepth;
        }

        public Viewport(Rectangle bounds) : this(bounds.X, bounds.Y, bounds.Width, bounds.Height)
        {

        }


        /// <summary>
        /// 设定&获取 视窗口的边界
        /// </summary>
        public Rectangle Bounds
        {
            get { return new Rectangle(x, y, width, height); }
            set
            {
                x = value.X;
                y = value.Y;
                width = value.Width;
                height = value.Height;
            }
        }
    }
}
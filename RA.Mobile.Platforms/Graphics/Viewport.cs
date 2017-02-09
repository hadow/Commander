using System;
using System.Collections.Generic;
namespace RA.Mobile.Platforms.Graphics
{
    
    public struct Viewport
    {

        private int x;
        private int y;
        private int width;
        private int height;
        private float minDepth;
        private float maxDepth;


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
    }
}
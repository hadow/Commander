using System;
using System.Collections.Generic;


namespace EW.Mobile.Platforms.Graphics
{
    /// <summary>
    /// 显示模式
    /// </summary>
    public class DisplayMode
    {


        private SurfaceFormat _format;

        public SurfaceFormat Format
        {
            get { return _format; }
        }
        private int _height;
        public int Height
        {
            get { return _height; }
        }
        private int _width;
        public int Width
        {
            get { return _width; }
        }


        /// <summary>
        /// 画面比例
        /// </summary>
        public float AspectRatio
        {
            get { return (float)_width / (float)_height; }
        }

        internal DisplayMode(int width,int height,SurfaceFormat format)
        {
            _width = width;
            _height = height;
            _format = format;
        }

        public static bool operator !=(DisplayMode left,DisplayMode right)
        {
            return !(left == right);
        }
        public static bool operator ==(DisplayMode left,DisplayMode right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return (left._format == right._format) && (left._width == right._width) && (left._height == right._height);
        }



    }
}
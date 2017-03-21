using System;
using System.Threading;

namespace EW.Mobile.Platforms.Graphics
{
    /// <summary>
    /// Œ∆¿ÌÃ˘Õº
    /// </summary>
    public abstract partial class Texture:GraphicsResource
    {

        internal SurfaceFormat _format;
        internal int _levelCount;

        private static int _lastSortingKey;

        private readonly int _sortingKey = Interlocked.Increment(ref _lastSortingKey);

        /// <summary>
        /// 
        /// </summary>
        internal int SortingKey
        {
            get { return _sortingKey; }
        }

        public SurfaceFormat Format
        {
            get { return _format; }
        }


        public int LevelCount
        {
            get { return _levelCount; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        internal static int CalculateMipLevels(int width,int height=0,int depth = 0)
        {
            int levels = 1;
            int size = Math.Max(Math.Max(width, height), depth);

            while (size > 1)
            {
                size /= 2;
                levels++;
            }
            return levels;

        }

        protected internal override void GraphicsDeviceResetting()
        {

        }


    }
}
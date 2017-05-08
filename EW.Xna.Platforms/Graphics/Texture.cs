using System;
using System.Threading;

namespace EW.Xna.Platforms.Graphics
{
    /// <summary>
    /// 纹理贴图
    /// </summary>
    public abstract partial class Texture:GraphicsResource
    {

        internal SurfaceFormat _format;
        internal int _levelCount;

        private static int _lastSortingKey;

        private readonly int _sortingKey = Interlocked.Increment(ref _lastSortingKey);

        /// <summary>
        /// Gets a unique identifier of this texture for sorting purpose
        /// 获取此纹理的唯一标识符以进行排序。
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
            PlatformGraphicsDeviceResetting();
        }


    }
}
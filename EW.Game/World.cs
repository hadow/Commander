using System;
using System.Collections.Generic;

namespace EW
{
    /// <summary>
    /// 虚拟战争世界
    /// </summary>
    public sealed class World:IDisposable
    {

        public readonly Map Map;

        uint nextAID = 0;

        internal uint NextAID()
        {
            return nextAID++;
        }

        public void Dispose()
        {
        }
    }
}
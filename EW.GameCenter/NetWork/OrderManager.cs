using System;

namespace EW.NetWork
{
    /// <summary>
    /// Õ¯¬Á÷∏¡Ó
    /// </summary>
    public sealed class OrderManager:IDisposable
    {
        public World World;


        public long lastTickTime;

        public int NetFrameNumber { get; private set; }
        public int LocalFrameNumber;


        public bool IsReadyForNextFrame
        {
            get { return NetFrameNumber >= 1; }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Tick()
        {

        }
        public void Dispose()
        {

        }
    }
}
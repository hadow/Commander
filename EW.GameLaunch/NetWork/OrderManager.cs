using System;
using System.Collections;
using System.Collections.Generic;
namespace EW.NetWork
{
    /// <summary>
    /// Õ¯¬Á÷∏¡Ó
    /// </summary>
    public sealed class OrderManager:IDisposable
    {
        public World World;

        public IConnection Connection { get; private set; }
        public long LastTickTime;

        public int NetFrameNumber { get; private set; }
        public int LocalFrameNumber;

        List<Order> localOrders = new List<Order>();

        public Session LobbyInfo = new Session();

        public bool GameStarted { get { return NetFrameNumber != 0; } }
        public void StartGame()
        {
            if (GameStarted)
                return;
            NetFrameNumber = 1;

        }


        public bool IsReadyForNextFrame
        {
            get { return NetFrameNumber >= 1; }
        }

        public void IssueOrder(Order order)
        {
            localOrders.Add(order);
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
using System;
using System.Collections.Generic;


namespace EW.NetWork
{
    public sealed class ReplayConnection:IConnection
    {

        public int LocalClientId { get { return 0; } }


        public ConnectionState ConnectionState { get { return ConnectionState.Connected; } }


        public void Send(int frame,List<byte[]> orders) { }

        public void SendImmediate(List<byte[]> orders) { }

        public void SendSync(int frame,byte[] syncData)
        {

        }

        public void Receive(Action<int,byte[]> packetFn)
        {

        }


        public void Dispose() { }
    }
}
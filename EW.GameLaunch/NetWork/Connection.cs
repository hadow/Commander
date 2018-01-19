using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace EW.NetWork
{

    public enum ConnectionState
    {
        PreConnecting,
        NotConnected,
        Connecting,
        Connected,
    }

    public interface IConnection : IDisposable
    {
        int LocalClientId { get; }

        ConnectionState ConnectionState { get; }

        void Send(int frame, List<byte[]> orders);

        void SendImmediate(List<byte[]> orders);

        void SendSync(int frame, byte[] syncData);

        void Receive(Action<int, byte[]> packetFn);
        

    }
    class Connection
    {
    }


    class EchoConnection:IConnection{

        protected struct ReceivedPacket{
            public int FromClient;
            public byte[] Data;

        }

        readonly List<ReceivedPacket> receivedPackets = new List<ReceivedPacket>();

        public virtual int LocalClientId{ get { return 1; }}

        public virtual ConnectionState ConnectionState{ get { return ConnectionState.PreConnecting; }}

        public virtual void Send(int frame,List<byte[]> orders)
        {
            var ms = new MemoryStream();
            ms.Write(BitConverter.GetBytes(frame));
            foreach (var o in orders)
                ms.Write(o);
            Send(ms.ToArray());

        }

        public virtual void SendImmediate(List<byte[]> orders){
            
        }

        public virtual void SendSync(int frame,byte[] syncData){

            var ms = new MemoryStream(4 + syncData.Length);
            ms.Write(BitConverter.GetBytes(frame));
            ms.Write(syncData);
            Send(ms.GetBuffer());
        }

        protected virtual void Send(byte[] packet){

            if (packet.Length == 0)
                throw new NotImplementedException();

            AddPacket(new ReceivedPacket { FromClient = LocalClientId, Data = packet });
            
        }

        protected void AddPacket(ReceivedPacket packet)
        {
            lock (receivedPackets)
                receivedPackets.Add(packet);

        }

        public virtual void Receive(Action<int,byte[]> packetFn){
            
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose(){
            
        }
    }
}
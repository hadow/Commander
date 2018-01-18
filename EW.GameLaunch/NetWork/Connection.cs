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
            public byte[] data;

        }

        public virtual int LocalClientId{ get { return 1; }}

        public virtual ConnectionState ConnectionState{ get { return ConnectionState.PreConnecting; }}

        public virtual void Send(int frame,List<byte[]> orders){
            
        }

        public virtual void SendImmediate(List<byte[]> orders){
            
        }

        public virtual void SendSync(int frame,byte[] syncData){
            
        }

        protected virtual void Send(byte[] packet){


        }

        public virtual void Receive(Action<int,byte[]> packetFn){
            
        }

        public void Dispose(){
            
        }
    }
}
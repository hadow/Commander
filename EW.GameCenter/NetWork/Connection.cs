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
}
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using EW.Server;
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

    class EchoConnection:IConnection
    {

        protected struct ReceivedPacket{
            public int FromClient;
            public byte[] Data;

        }

        readonly List<ReceivedPacket> receivedPackets = new List<ReceivedPacket>();

        public virtual int LocalClientId{ get { return 1; }}

        public virtual ConnectionState ConnectionState{ get { return ConnectionState.PreConnecting; }}

        public ReplayRecorder Recorder { get; private set; }
        
        public virtual void Send(int frame,List<byte[]> orders)
        {
            var ms = new MemoryStream();
            ms.Write(BitConverter.GetBytes(frame));
            foreach (var o in orders)
                ms.Write(o);
            Send(ms.ToArray());

        }

        public virtual void SendImmediate(List<byte[]> orders)
        {
            var ms = new MemoryStream();
            ms.Write(BitConverter.GetBytes(0));
            foreach (var o in orders)
                ms.Write(o);
            Send(ms.ToArray());
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

        public virtual void Receive(Action<int,byte[]> packetFn)
        {
            ReceivedPacket[] packets;
            lock (receivedPackets)
            {
                packets = receivedPackets.ToArray();
                receivedPackets.Clear();
            }

            foreach(var p in packets)
            {
                packetFn(p.FromClient, p.Data);
                if (Recorder != null)
                    Recorder.Receive(p.FromClient, p.Data);
            }
        }

        public void StartRecording(Func<string> chooseFilename)
        {

        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && Recorder != null)
                Recorder.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }


    sealed class NetworkConnection : EchoConnection
    {
        readonly TcpClient tcp;
        readonly List<byte[]> queuedSyncPackets = new List<byte[]>();
        volatile ConnectionState connectionState = ConnectionState.Connecting;
        volatile int clientId;
        bool disposed;


        public override int LocalClientId { get { return clientId; } }

        public override ConnectionState ConnectionState { get { return connectionState; } }

        public override void SendSync(int frame, byte[] syncData)
        {
            var ms = new MemoryStream(4 + syncData.Length);
            ms.Write(BitConverter.GetBytes(frame));
            ms.Write(syncData);
            queuedSyncPackets.Add(ms.GetBuffer());
        }

        protected override void Send(byte[] packet)
        {
            base.Send(packet);

            try
            {
                var ms = new MemoryStream();
                ms.Write(BitConverter.GetBytes(packet.Length));
                ms.Write(packet);

                foreach(var q in queuedSyncPackets)
                {
                    ms.Write(BitConverter.GetBytes(q.Length));
                    ms.Write(q);
                    base.Send(q);
                }

                queuedSyncPackets.Clear();
                ms.WriteTo(tcp.GetStream());
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
            catch (IOException) { }
        }


        public NetworkConnection(string host,int port)
        {
            try
            {
                tcp = new TcpClient(host, port) { NoDelay = true };
                new Thread(NetworkConnectionReceive)
                {
                    Name = GetType().Name + " " + host + ":" + port,
                    IsBackground = true
                }.Start(tcp.GetStream());
            }
            catch
            {
                connectionState = ConnectionState.NotConnected;
            }
        }

        void NetworkConnectionReceive(object networkStreamObject)
        {
            try
            {
                var networkStream = (NetworkStream)networkStreamObject;
                var reader = new BinaryReader(networkStream);
                var serverProtocol = reader.ReadInt32();

                if (ProtocolVersion.Version != serverProtocol)
                    throw new InvalidOperationException("Protocol version mismatch.Server={0} Client={1}".F(serverProtocol, ProtocolVersion.Version));

                clientId = reader.ReadInt32();
                connectionState = ConnectionState.Connected;

                for(; ; )
                {
                    var len = reader.ReadInt32();
                    var client = reader.ReadInt32();
                    var buf = reader.ReadBytes(len);
                    if (len == 0)
                        throw new NotImplementedException();

                    AddPacket(new ReceivedPacket { FromClient = client, Data = buf });
                }

            }
            catch { }
            finally
            {
                connectionState = ConnectionState.NotConnected;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            disposed = true;

            // Closing the stream will cause any reads on the receiving thread to throw.
            // This will mark the connection as no longer connected and the thread will terminate cleanly.
            //关闭流会导致接收线程上的任何读取操作抛出,这会将连接标记为不再连接,并且线程将干净地终止
            if (tcp != null)
                tcp.Close();

            base.Dispose(disposing);
        }
    }
}
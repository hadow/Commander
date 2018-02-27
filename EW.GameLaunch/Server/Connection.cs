using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace EW.Server
{
    public enum ReceiveState { Header,Data}
    public class Connection
    {
        public const int MaxOrderLength = 131072;
        
        public Socket Socket;
        public List<byte> Data = new List<byte>();
        public ReceiveState State = ReceiveState.Header;
        public int ExternalLength = 8;
        public int Frame = 0;
        public int ExpectLength = 8;
        public int MostRecentFrame = 0;
        public bool Validated;

        long lastReceivedTime = 0;

        public long TimeSinceLastResponse { get { return WarGame.RunTime - lastReceivedTime; } }
        public bool TimeoutMessageShown = false;

        public int PlayerIndex;

        public byte[] PopBytes(int n)
        {
            var result = Data.GetRange(0, n);
            Data.RemoveRange(0, n);
            return result.ToArray();
        }

        public void ReadData(Server server)
        {
            if(ReadDataInner(server))
                while(Data.Count >= ExpectLength)
                {
                    var bytes = PopBytes(ExpectLength);
                    switch (State)
                    {
                        case ReceiveState.Header:
                            {
                                ExpectLength = BitConverter.ToInt32(bytes, 0) - 4;
                                Frame = BitConverter.ToInt32(bytes, 4);
                                State = ReceiveState.Data;

                                if(ExpectLength<0 || ExpectLength > MaxOrderLength)
                                {
                                    server.DropClient(this);
                                    return;
                                }
                            }
                            break;
                        case ReceiveState.Data:
                            {
                                if (MostRecentFrame < Frame)
                                    MostRecentFrame = Frame;

                                server.DispatchOrders(this, Frame, bytes);
                                ExpectLength = 8;
                                State = ReceiveState.Header;
                            }
                            break;
                    }
                }
        }


        bool ReadDataInner(Server server)
        {

            var rx = new byte[1024];
            var len = 0;

            for(; ; )
            {
                try
                {
                    //Poll the socket first to see if there's anything there.
                    if (!Socket.Poll(0, SelectMode.SelectRead))
                        break;

                    if (0 < (len = Socket.Receive(rx)))
                        Data.AddRange(rx.Take(len));
                    else
                    {
                        if (len == 0)
                            server.DropClient(this);
                        break;
                    }
                }
                catch(SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock)
                        break;

                    server.DropClient(this);
                    return false;
                }
            }

            lastReceivedTime = WarGame.RunTime;
            TimeoutMessageShown = false;

            return true;
        }

    }
}
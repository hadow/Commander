using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using EW.Support;
using EW.NetWork;
using EW.Primitives;
using EW.Traits;
namespace EW.Server
{
    public abstract class ServerTrait { }

    public enum ServerState
    {
        WaitingPlayers = 1,
        GameStarted =2,
        ShuttingDown = 3,
    }

    public class Server
    {
        public readonly IPAddress Ip;
        public readonly int Port;
        public readonly MersenneTwister Random = new MersenneTwister();
        public readonly bool Dedicated;

        //Valid player connectionis
        public List<Connection> Conns = new List<Connection>();

        //Pre-verified player connections
        public List<Connection> PreConns = new List<Connection>();

        public Session LobbyInfo;
        public ServerSettings Settings;

        public ModData ModData;

        public List<string> TempBans = new List<string>();

        public MapPreview Map;

        readonly int randomSeed;
        readonly TcpListener listener;
        readonly TypeDictionary serverTraits = new TypeDictionary();

        protected volatile ServerState internalState = ServerState.WaitingPlayers;

        public ServerState State
        {
            get { return internalState; }
            set
            {
                internalState = value;
            }
        }

        public Server(IPEndPoint endpoint,ServerSettings settings,ModData modData,bool dedicated)
        {
            listener = new TcpListener(endpoint);
            listener.Start();
            var localEndpoint = (IPEndPoint)listener.LocalEndpoint;
            Ip = localEndpoint.Address;
            Port = localEndpoint.Port;
            Dedicated = dedicated;
            Settings = settings;

            ModData = modData;

            randomSeed = (int)DateTime.Now.ToBinary();

            if(UPnP.Status == UPnPStatus.Enabled)
            {
                UPnP.ForwardPort(Settings.ListenPort, Settings.ExternalPort).Wait();
            }

            foreach (var trait in modData.Manifest.ServerTraits)
                serverTraits.Add(modData.ObjectCreator.CreateObject<ServerTrait>(trait));

            LobbyInfo = new Session
            {
                GlobalSettings =
                {
                    RandomSeed = randomSeed,
                    Map = settings.Map,
                    ServerName = settings.Name,
                    EnableSinglePlayer = settings.EnableSingleplayer || !dedicated,
                    GameUid = Guid.NewGuid().ToString()
                }
            };

            new Thread(_ =>
            {
                foreach (var t in serverTraits.WithInterface<INotifyServerStart>())
                    t.ServerStarted(this);

                var timeout = serverTraits.WithInterface<ITick>().Min(t => t.TickTimeout);
                for (; ; )
                {
                    var checkRead = new List<Socket>();
                    if (State == ServerState.WaitingPlayers)
                        checkRead.Add(listener.Server);

                    checkRead.AddRange(Conns.Select(c => c.Socket));
                    checkRead.AddRange(PreConns.Select(c => c.Socket));

                    if (checkRead.Count > 0)
                        Socket.Select(checkRead, null, null, timeout);

                    if (State == ServerState.ShuttingDown)
                    {
                        EndGame();
                        break;
                    }

                    foreach (var s in checkRead)
                    {
                        if (s == listener.Server)
                        {
                            AcceptConnection();
                            continue;
                        }

                        var preConn = PreConns.SingleOrDefault(c => c.Socket == s);
                        if (preConn != null)
                        {
                            preConn.ReadData(this);
                            continue;
                        }

                        var conn = Conns.SingleOrDefault(c => c.Socket == s);
                        if (conn != null)
                            conn.ReadData(this);
                    }


                    foreach (var t in serverTraits.WithInterface<ITick>())
                        t.Tick(this);

                    if (State == ServerState.ShuttingDown)
                    {
                        EndGame();
                        if (UPnP.Status == UPnPStatus.Enabled)
                        {
                            UPnP.RemovePortForward().Wait();
                        }
                        break;
                    }
                }

                foreach (var t in serverTraits.WithInterface<INotifyServerShutdown>())
                    t.ServerShutdown(this);

                PreConns.Clear();
                Conns.Clear();

                try
                {
                    listener.Stop();
                }
                catch { }
            }
            )
            {
                IsBackground = true
            }.Start();
        }

        public void StartGame()
        {
            listener.Stop();

            //Drop any unvalidated clients
            foreach (var c in PreConns.ToArray())
                DropClient(c);

            //Drop any players who are not ready.
            foreach(var c in Conns.Where(c=>GetClient(c).IsInvalid).ToArray())
            {
                SendOrderTo(c, "ServerError", "You have been kicked from the server!");
                DropClient(c);
            }

            if (LobbyInfo.NonBotClients.Count() == 1)
                LobbyInfo.GlobalSettings.OrderLatency = 1;

            SyncLobbyInfo();
            State = ServerState.GameStarted;

            foreach (var c in Conns)
                foreach (var d in Conns)
                    DispatchOrdersToClient(c, d.PlayerIndex, 0x7FFFFFFF, new byte[] { 0xBF });

            DispatchOrders(null, 0, new ServerOrder("StartGame", "").Serialize());

            foreach (var t in serverTraits.WithInterface<IStartGame>())
                t.GameStarted(this);
        }

        public Session.Client GetClient(Connection conn)
        {
            return LobbyInfo.ClientWithIndex(conn.PlayerIndex);
        }

        void AcceptConnection()
        {
            Socket newSocket;

            try
            {
                if (!listener.Server.IsBound)
                    return;

                newSocket = listener.AcceptSocket();
            }
            catch(Exception exp)
            {
                return;
            }

            var newConn = new Connection { Socket = newSocket };

            try
            {
                newConn.Socket.Blocking = false;
                newConn.Socket.NoDelay = true;

            }
            catch(Exception e)
            {

            }
        }

        public void SendMessage(string text)
        {
            DispatchOrdersToClients(null, 0, new ServerOrder("Message", text).Serialize());
        }

        public void SendOrderTo(Connection conn,string order,string data)
        {
            DispatchOrdersToClient(conn, 0, 0, new ServerOrder(order, data).Serialize());

            if (Dedicated)
            {

            }
        }

        public void DispatchOrdersToClients(Connection conn,int frame,byte[] data)
        {
            var from = conn != null ? conn.PlayerIndex : 0;
            foreach (var c in Conns.Except(conn).ToList())
                DispatchOrdersToClient(c, from, frame, data);
        }

        void DispatchOrdersToClient(Connection c,int client,int frame,byte[] data)
        {
            try
            {
                SendData(c.Socket, BitConverter.GetBytes(data.Length + 4));
                SendData(c.Socket, BitConverter.GetBytes(client));
                SendData(c.Socket, BitConverter.GetBytes(frame));
                SendData(c.Socket, data);
            }
            catch(Exception e)
            {
                DropClient(c);
            }
        }

        public void DispatchOrders(Connection conn,int frame,byte[] data)
        {
            if(frame == 0 && conn != null)
            {
                InterpretServerOrders(conn, data);
            }
            else
            {
                DispatchOrdersToClients(conn, frame, data);
            }
        }

        void InterpretServerOrders(Connection conn,byte[] data)
        {
            var ms = new MemoryStream(data);
            var br = new BinaryReader(ms);

            try
            {
                while(ms.Position < ms.Length)
                {
                    var so = ServerOrder.Deserialize(br);
                    if (so == null) return;
                    InterpretServerOrder(conn, so);
                }
            }
            catch (EndOfStreamException) { }
            catch (NotImplementedException) { }
        }

        void InterpretServerOrder(Connection conn,ServerOrder so)
        {

        }

        public void DropClient(Connection toDrop)
        {
            if (!PreConns.Remove(toDrop))
            {
                Conns.Remove(toDrop);

                var dropClient = LobbyInfo.Clients.FirstOrDefault(cl => cl.Index == toDrop.PlayerIndex);
                if (dropClient == null)
                    return;

                var suffix = "";
                if (State == ServerState.GameStarted)
                    suffix = dropClient.IsObserver ? "(Spectator)" : dropClient.Team != 0 ? " (Team {0})".F(dropClient.Team) : "";

                SendMessage("{0}{1} has disconnected.".F(dropClient.Name, suffix));


                //Send disconnected order,even if still in the lobby
                DispatchOrdersToClients(toDrop, 0, new ServerOrder("Disconnected", "").Serialize());

                LobbyInfo.Clients.RemoveAll(c => c.Index == toDrop.PlayerIndex);
                LobbyInfo.ClientPings.RemoveAll(p => p.Index == toDrop.PlayerIndex);

                if(Dedicated && dropClient.IsAdmin && State == ServerState.WaitingPlayers)
                {
                    //Remove any bots controlled by the admin
                    LobbyInfo.Clients.RemoveAll(c => c.Bot != null && c.BotControllerClientIndex == toDrop.PlayerIndex);

                    var nextAdmin = LobbyInfo.Clients.Where(cl => cl.Bot == null).MinByOrDefault(c => c.Index);

                    if(nextAdmin != null)
                    {
                        nextAdmin.IsAdmin = true;
                        SendMessage("{0} is now the admin.".F(nextAdmin.Name));
                    }
                }

                DispatchOrders(toDrop, toDrop.MostRecentFrame, new byte[] { 0xbf });

                if (!Conns.Any())
                    foreach (var t in serverTraits.WithInterface<INotifyServerEmpty>())
                        t.ServerEmpty(this);

                if (Conns.Any() || Dedicated)
                    SyncLobbyClients();

                if (!Dedicated && dropClient.IsAdmin)
                    Shutdown();
            }

            try
            {
                toDrop.Socket.Disconnect(false);
            }
            catch { }
        }

        public void SyncLobbyClients()
        {

        }

        public void SyncLobbyInfo()
        {

        }

        static void SendData(Socket s,byte[] data)
        {
            var start = 0;
            var length = data.Length;

            //Non-blocking sends are free to send only part of the data.
            while(start < length)
            {
                SocketError error;
                var sent = s.Send(data, start, length - start, SocketFlags.None, out error);
                if (error == SocketError.WouldBlock)
                {
                    //Non-blocking send of bytes failed.Falling back to blocking send.
                    s.Blocking = true;
                    sent = s.Send(data, start, length - start, SocketFlags.None);
                    s.Blocking = false;
                }
                else if (error != SocketError.Success)
                    throw new SocketException((int)error);

                start += sent;
            }
        }


        public void EndGame()
        {
            foreach (var t in serverTraits.WithInterface<IEndGame>())
                t.GameEnded(this);
        }

        public void Shutdown()
        {
            State = ServerState.ShuttingDown;
        }

    }
}
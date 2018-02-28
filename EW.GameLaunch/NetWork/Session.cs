using System;
using System.Collections.Generic;
using System.Linq;
using EW.Graphics;
namespace EW.NetWork
{
    public class Session
    {
        public List<Client> Clients = new List<Client>();
        public List<ClientPing> ClientPings = new List<ClientPing>();
        public Global GlobalSettings = new Global();

        //Keyed by the PlayerReference id that the slot corresponds to 
        public Dictionary<string, Slot> Slots = new Dictionary<string, Slot>();

        public IEnumerable<Client> NonBotClients
        {
            get
            {
                return Clients.Where(c => c.Bot == null);
            }
        }

        public IEnumerable<Client> NonBotPlayers
        {
            get { return Clients.Where(c => c.Bot == null && c.Slot != null); }
        }

        public Client ClientWithIndex(int clientID){
            return Clients.SingleOrDefault(c => c.Index == clientID);
        }

        public Client ClientInSlot(string slot)
        {
            return Clients.SingleOrDefault(c => c.Slot == slot);
        }

        public static Session Deserialize(string data)
        {
            try
            {
                var session = new Session();

                var nodes = MiniYaml.FromString(data);

                foreach(var node in nodes)
                {
                    var strings = node.Key.Split('@');

                    switch (strings[0])
                    {
                        case "Client":
                            session.Clients.Add(Client.Deserialize(node.Value));
                            break;
                        case "ClientPing":
                            session.ClientPings.Add(ClientPing.Deserialize(node.Value));
                            break;

                        case "GlobalSettings":
                            session.GlobalSettings = Global.Deserialize(node.Value);
                            break;

                        case "Slot":
                            var s = Slot.Deserialize(node.Value);
                            session.Slots.Add(s.PlayerReference, s);
                            break;
                    }
                }

                return session;
            }
            catch (YamlException)
            {
                throw new YamlException("Session deserialized invalid MiniYaml:\n{0}".F(data));
            }
            catch (InvalidOperationException)
            {
                throw new YamlException("Session deserialized invalid MiniYaml:\n{0}".F(data));
            }
        }

        public enum ClientState{
            NotReady,
            Invalid,
            Ready,
            Disconnected = 1000,
        }

        public class Client
        {

            public HSLColor PreferredColor;
            public HSLColor Color;

            public ClientState State = ClientState.Invalid;
            public int Team;
            public int Index;

            public string Faction;

            public int SpawnPoint;

            public string Name;

            public string IpAddress;

            public string Slot;//Slot ID,or null for observer

            public string Bot;//Bot type,null for real client.

            public bool IsAdmin;

            public int BotControllerClientIndex;//who added the bot to the slot

            public bool IsReady{ get { return State == ClientState.Ready; }}

            public bool IsInvalid{ get { return State == ClientState.Invalid; }}

            public bool IsObserver{ get { return Slot == null; }}

            public MiniYamlNode Serialize()
            {
                return new MiniYamlNode("Client@{0}".F(Index), FieldSaver.Save(this));
            }

            public static Client Deserialize(MiniYaml data)
            {
                return FieldLoader.Load<Client>(data);
            }
        }
        public class ClientPing
        {
            public int Index;
            public long Latency = -1;
            public long LatencyJitter = -1;//延迟抖动
            public long[] LatencyHistory = { };

            public static ClientPing Deserialize(MiniYaml data)
            {
                return FieldLoader.Load<ClientPing>(data);
            }


            public MiniYamlNode Serialize()
            {
                return new MiniYamlNode("ClientPing@{0}".F(Index), FieldSaver.Save(this));
            }


        }

        public class Slot
        {
            public string PlayerReference;
            public bool Closed;

            public bool AllowBots;
            public bool LockFaction;
            public bool LockColor;
            public bool LockSpawn;
            public bool LockTeam;
            public bool Required;

            public static  Slot Deserialize(MiniYaml data)
            {
                return FieldLoader.Load<Slot>(data);
            }

            public MiniYamlNode Serialize()
            {
                return new MiniYamlNode("Slot@{0}".F(PlayerReference), FieldSaver.Save(this));
            }
        }


        public class LobbyOptionState
        {
            public string value;
            public string PreferredValue;

            public bool IsLocked;

            public bool IsEnabled { get { return value == "True"; } }
        }


        public class Global
        {
            public string Map;
            public string ServerName;
            public int Timestep = 40;
            public int OrderLatency = 3;
            public int RandomSeed = 0;
            public bool EnableSinglePlayer;
            public bool AllowSpectators = true;
            public bool AllowVersionMismatch;
            public string GameUid;

            [FieldLoader.Ignore]
            public Dictionary<string, LobbyOptionState> LobbyOptions = new Dictionary<string, LobbyOptionState>();

            public string OptionOrDefault(string id,string def)
            {
                LobbyOptionState option;
                if (LobbyOptions.TryGetValue(id, out option))
                    return option.value;

                return def;
            }

            public bool OptionOrDefault(string id,bool def)
            {
                LobbyOptionState option;
                if (LobbyOptions.TryGetValue(id, out option))
                    return option.IsEnabled;

                return def;
            }


            public static Global Deserialize(MiniYaml data)
            {
                var gs = FieldLoader.Load<Global>(data);

                var optionsNode = data.Nodes.FirstOrDefault(n => n.Key == "Options");
                if (optionsNode != null)
                    foreach (var n in optionsNode.Value.Nodes)
                        gs.LobbyOptions[n.Key] = FieldLoader.Load<LobbyOptionState>(n.Value);

                return gs;
            }

            public MiniYamlNode Serialize()
            {
                var data = new MiniYamlNode("GlobalSettings", FieldSaver.Save(this));
                var options = LobbyOptions.Select(kv => new MiniYamlNode(kv.Key, FieldSaver.Save(kv.Value))).ToList();
                data.Value.Nodes.Add(new MiniYamlNode("Options", new MiniYaml(null, options)));

                return data;
            }
        }

        public ClientPing PingFromClient(Client client)
        {
            return ClientPings.SingleOrDefault(p => p.Index == client.Index);
        }


        public string Serialize()
        {
            var sessionData = new List<MiniYamlNode>();

            foreach (var client in Clients)
                sessionData.Add(client.Serialize());

            foreach (var clientPing in ClientPings)
                sessionData.Add(clientPing.Serialize());

            foreach (var slot in Slots)
                sessionData.Add(slot.Value.Serialize());

            sessionData.Add(GlobalSettings.Serialize());

            return sessionData.WriteToString();
        }

        public string FirstEmptySlot()
        {
            return Slots.FirstOrDefault(s => !s.Value.Closed && ClientInSlot(s.Key) == null).Key;
        }
    }
}
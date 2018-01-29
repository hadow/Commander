using System;
using System.Collections.Generic;
using System.Linq;
namespace EW.NetWork
{
    public class Session
    {
        public List<Client> Clients = new List<Client>();

        public Global GlobalSettings = new Global();

        //Keyed by the PlayerReference id that the slot corresponds to 
        public Dictionary<string, Slot> Slots = new Dictionary<string, Slot>();


        public Client ClientWithIndex(int clientID){
            return Clients.SingleOrDefault(c => c.Index == clientID);
        }

        public Client ClientInSlot(string slot)
        {
            return Clients.SingleOrDefault(c => c.Slot == slot);
        }

        public enum ClientState{
            NotReady,
            Invalid,
            Ready,
            Disconnected = 1000,
        }

        public class Client
        {

            public ClientState State = ClientState.Invalid;

            public int Index;

            public string Faction;

            public int SpawnPoint;

            public string Name;

            public string IpAddress;

            public string Slot;

            public string Bot;//Bot type,null for real client.

            public bool IsAdmin;

            public int BotControllerClientIndex;//who added the bot to the slot

            public bool IsReady{ get { return State == ClientState.Ready; }}

            public bool IsInvalid{ get { return State == ClientState.Invalid; }}

            public bool IsObserver{ get { return Slot == null; }}
        }
        public class ClientPing
        {

        }

        public class Slot
        {
            public string PlayerReference;
            public bool Closed;
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
            public int RandomSeed = 0;
            public bool EnableSinglePlayer;
            public bool AllowSpectators = true;

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
        }
    }
}
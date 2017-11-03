using System;

namespace EW.NetWork
{
    public class Session
    {

        public Global GlobalSettings = new Global();


        public class Client
        {
            public int Index;

            public string Faction;

            public int SpawnPoint;

            public string Name;

            public string IpAddress;

            public string Slot;

            public string Bot;

            public bool IsAdmin;


        }
        public class ClientPing
        {

        }

        public class Slot
        {

        }



        public class Global
        {
            public string Map;
            public string ServerName;
            public int Timestep = 40;
            public int RandomSeed = 0;
            public bool EnableSinglePlayer;
            public bool AllowSpectators = true;
        }
    }
}
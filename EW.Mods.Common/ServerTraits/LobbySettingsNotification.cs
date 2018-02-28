using System;
using EW.NetWork;
using EW.Server;
using EW.Traits;

namespace EW.Mods.Common.Server
{
    public class LobbySettingsNotification:ServerTrait,IClientJoined
    {

        void IClientJoined.ClientJoined(EW.Server.Server server, EW.Server.Connection conn)
        {

        }

    }
}
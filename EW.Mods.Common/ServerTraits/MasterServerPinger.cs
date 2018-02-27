using System;
using System.Collections.Generic;
using System.Linq;

using EW.NetWork;
using EW.Server;
using System.Net;
using S = EW.Server.Server;
namespace EW.Mods.Common.Server
{
    public class MasterServerPinger:ServerTrait,ITick,INotifyServerStart
    {

        const int MasterPingInterval = 60 * 3;

        public int TickTimeout { get { return MasterPingInterval * 10000; } }

        void ITick.Tick(S server)
        {

        }

        public void ServerStarted(S server)
        {

        }


    }
}
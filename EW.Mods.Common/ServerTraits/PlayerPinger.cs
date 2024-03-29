﻿using System;
using System.Linq;
using EW.Server;
using S = EW.Server.Server;
namespace EW.Mods.Common.Server
{
    public class PlayerPinger:ServerTrait,ITick
    {
        //Ping every 5 seconds.
        static readonly int PingInterval = 5000;
        //Report every 20 seconds.
        static readonly int ConnReportInterval = 20000;
        //Drop unresponsive clients after 60 seconds.
        static readonly int ConnTimeout = 60000;

        long lastPing = 0;
        long lastConnReport = 0;
        bool isInitialPing = true;

        void ITick.Tick(S server)
        {
            if((WarGame.RunTime - lastPing)>PingInterval || isInitialPing)
            {
                isInitialPing = false;
                lastPing = WarGame.RunTime;

                //Ignore client timeout in singleplayer games to make debugging easier
                if (server.LobbyInfo.NonBotClients.Count() < 2 && !server.Dedicated)
                    foreach (var c in server.Conns.ToList())
                        server.SendOrderTo(c, "Ping", WarGame.RunTime.ToString());
                else
                {
                    foreach(var c in server.Conns.ToList())
                    {
                        if (c == null || c.Socket == null)
                            continue;

                        var client = server.GetClient(c);
                        if(client == null)
                        {
                            server.DropClient(c);
                            server.SendMessage("A player has been dropped after timing out.");
                            continue;
                        }

                        if(c.TimeSinceLastResponse < ConnTimeout)
                        {
                            server.SendOrderTo(c, "Ping", WarGame.RunTime.ToString());
                            if(!c.TimeoutMessageShown && c.TimeSinceLastResponse > PingInterval * 2)
                            {
                                server.SendMessage(client.Name + " is experiencing connection problems.");
                                c.TimeoutMessageShown = true;
                            }
                        }
                        else
                        {
                            server.SendMessage(client.Name + " has benn dropped after timing out.");
                            server.DropClient(c);
                        }
                    }
                }

                if(WarGame.RunTime - lastConnReport > ConnReportInterval)
                {
                    lastConnReport = WarGame.RunTime;

                    var timeouts = server.Conns.Where(c => c.TimeSinceLastResponse > ConnReportInterval && c.TimeSinceLastResponse < ConnTimeout).OrderBy(c => c.TimeSinceLastResponse);

                    foreach(var c in timeouts)
                    {
                        if (c == null || c.Socket == null)
                            continue;

                        var client = server.GetClient(c);
                        if (client != null)
                            server.SendMessage("{0} will be dropped in {1} seconds.".F(client.Name, (ConnTimeout - c.TimeSinceLastResponse) / 1000));
                    }
                }

            }
        }

        public int TickTimeout { get { return PingInterval * 100; } }



    }
}
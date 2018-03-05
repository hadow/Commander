using System;
using System.Collections.Generic;
using EW.Traits;
using System.Linq;
namespace EW.NetWork
{
    public static class UnitOrders
    {
        static void SetOrderLag(OrderManager o)
        {
            if(o.FramesAhead != o.LobbyInfo.GlobalSettings.OrderLatency && !o.GameStarted)
            {
                o.FramesAhead = o.LobbyInfo.GlobalSettings.OrderLatency;
            }
        }

        /// <summary>
        /// Processes the order.
        /// </summary>
        /// <param name="orderManager">Order manager.</param>
        /// <param name="world">World.</param>
        /// <param name="clientId">Client identifier.</param>
        /// <param name="order">Order.</param>
        internal static void ProcessOrder(OrderManager orderManager,World world,int clientId,Order order)
        {

            if(world != null){

                if(!world.WorldActor.TraitsImplementing<IValidateOrder>().All(vo=>vo.OrderValidation(orderManager,world,clientId,order))){
                    return;
                }
            }

            switch(order.OrderString){
                case "Message":
                    {

                        break;
                    }
                case "Disconnected":
                    {
                        var client = orderManager.LobbyInfo.ClientWithIndex(clientId);
                        if (client != null)
                            client.State = Session.ClientState.Disconnected;
                        break;
                    }
                case "StartGame":
                    {
                        if(WarGame.ModData.MapCache[orderManager.LobbyInfo.GlobalSettings.Map].Status != MapStatus.Available)
                        {
                            WarGame.Disconnect();
                            WarGame.LoadShellMap();
                            break;
                        }
                        WarGame.StartGame(orderManager.LobbyInfo.GlobalSettings.Map, WorldT.Regular);

                        break;
                    }
                case "PauseGame":
                    {
                        var client = orderManager.LobbyInfo.ClientWithIndex(clientId);
                        if(client != null){

                            var pause = order.TargetString == "Pause";
                            if(orderManager.World.Paused != pause && world != null && world.LobbyInfo.NonBotClients.Count()>1){


                            }
                            orderManager.World.Paused = pause;
                            orderManager.World.PredictedPaused = pause;
                        }

                        break;
                    }
                case "HandshakeRequest":
                    {
                        var mod = WarGame.ModData.Manifest;
                        var request = HandshakeRequest.Deserialize(order.TargetString);

                        WarGame.Settings.Player.Name = Settings.SanitizedPlayerName(WarGame.Settings.Player.Name);

                        var info = new Session.Client()
                        {
                            Name = WarGame.Settings.Player.Name,
                            PreferredColor = WarGame.Settings.Player.Color,
                            Color = WarGame.Settings.Player.Color,
                            Faction = "Random",
                            SpawnPoint = 0,
                            Team = 0,
                            State = Session.ClientState.Invalid
                        };

                        var response = new HandshakeResponse()
                        {
                            Client = info,
                            Mod = mod.Id,
                            Version = mod.Metadata.Version,
                            Password = orderManager.Password,
                        };

                        orderManager.IssueOrder(Order.HandshakeResponse(response.Serialize()));
                        
                        break;
                    }
                case "ServerError":
                    {
                        orderManager.ServerError = order.TargetString;
                        orderManager.AuthenticationFailed = false;
                        break;
                    }
                case "AuthenticationError":{
                        //
                        orderManager.ServerError = order.TargetString;
                        orderManager.AuthenticationFailed = false;
                        break;
                    }
                case "SyncInfo":
                    {
                        orderManager.LobbyInfo = Session.Deserialize(order.TargetString);
                        SetOrderLag(orderManager);
                        WarGame.SyncLobbyInfo();
                        break;
                    }
                case "SyncClientPings":
                    {
                        var pings = new List<Session.ClientPing>();
                        var nodes = MiniYaml.FromString(order.TargetString);
                        foreach(var node in nodes)
                        {
                            var strings = node.Key.Split('@');
                            if (strings[0] == "ClientPing")
                                pings.Add(Session.ClientPing.Deserialize(node.Value));
                        }

                        orderManager.LobbyInfo.ClientPings = pings;
                        break;
                    }
                case "SyncLobbyGlobalSettings":
                    {
                        var nodes = MiniYaml.FromString(order.TargetString);
                        foreach(var node in nodes){
                            var strings = node.Key.Split('@');
                            if (strings[0] == "GlobalSettings")
                                orderManager.LobbyInfo.GlobalSettings = Session.Global.Deserialize(node.Value);
                        }
                        SetOrderLag(orderManager);
                        WarGame.SyncLobbyInfo();
                        break;
                    }
                case "Ping":
                    {
                        orderManager.IssueOrder(Order.Pong(order.TargetString));
                        break;
                    }

                default:
                    {
                        if(!order.IsImmediate){
                            var self = order.Subject;
                            if(!self.IsDead)
                            {
                                foreach (var t in self.TraitsImplementing<IResolveOrder>())
                                    t.ResolveOrder(self, order);
                            }
                        }
                        break;
                    }
            }
        }
    }
}

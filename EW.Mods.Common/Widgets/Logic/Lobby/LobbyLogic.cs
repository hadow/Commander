using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using EW.Widgets;
using EW.Traits;
using EW.NetWork;
using EW.Graphics;
using EW.NetWork;
namespace EW.Mods.Common.Widgets.Logic
{
    public class LobbyLogic:ChromeLogic
    {
        readonly ModData modData;
        readonly Action onStart;
        readonly Action onExit;
        readonly OrderManager orderManager;
        readonly bool skirmishMode;
        readonly Ruleset modRules;
        readonly World shellmapWorld;
        readonly WebServices services;

        MapPreview map;
        bool addBotOnMapLoad;
        readonly Widget lobby;


        [ObjectCreator.UseCtor]
        internal LobbyLogic(Widget widget, ModData modData, WorldRenderer worldRenderer, OrderManager orderManager, Action onExit, Action onStart, bool skirmishMode)
        {

            map = MapCache.UnknownMap;
            lobby = widget;
            this.modData = modData;
            this.orderManager = orderManager;
            this.onStart = onStart;
            this.onExit = onExit;
            this.skirmishMode = skirmishMode;


            services = modData.Manifest.Get<WebServices>();

            WarGame.LobbyInfoChanged += UpdateCurrentMap;
            WarGame.LobbyInfoChanged += UpdatePlayerList;
            WarGame.BeforeGameStart += OnGameStart;
            WarGame.ConnectionStateChanged += ConnectionStateChanged;


            UpdateCurrentMap();

            var gameStarting = false;


            var mapButton = lobby.GetOrNull<ButtonWidget>("CHANGEMAP_BUTTON");

            Action startGame = () =>
             {
                 gameStarting = true;
                 orderManager.IssueOrder(Order.Command("startgame"));
             };

            var startGameButton = lobby.GetOrNull<ButtonWidget>("START_GAME_BUTTON");

            if(startGameButton!=null){

                startGameButton.OnClick = () =>
                {
                    if(orderManager.LobbyInfo.Clients.Any(c=>c.Slot != null && !c.IsAdmin && c.Bot == null && !c.IsReady))
                    {
                        
                    }
                    else{
                        startGame();
                    }
                };
            }

            var disconnectButton = lobby.Get<ButtonWidget>("DISCONNECT_BUTTON");
            disconnectButton.OnClick = () => { UI.CloseWindow(); onExit(); };

            if (skirmishMode)
                disconnectButton.Text = "Back";

            // Add a bot on the first lobbyinfo update
            if (skirmishMode)
                addBotOnMapLoad = true;
        }

        void OnGameStart()
        {
            UI.CloseWindow();
            onStart();
        }

        void UpdateCurrentMap() 
        {

            var uid = orderManager.LobbyInfo.GlobalSettings.Map;
            if (map.Uid == uid)
                return;

            map = modData.MapCache[uid];
            if(map.Status == MapStatus.Available){

                // Maps need to be validated and pre-loaded before they can be accessed
                var currentMap = map;
                new Task(() =>
                {
                    // Force map rules to be loaded on this background thread
                    currentMap.PreloadRules();

                    WarGame.RunAfterTick(() =>
                    {
                        // Map may have changed in the meantime
                        if (currentMap != map)
                            return;

                        // Tell the server that we have the map
                        if (!currentMap.InvalidCustomRules)
                            orderManager.IssueOrder(Order.Command("state {0}".F(Session.ClientState.NotReady)));


                        if(addBotOnMapLoad){

                            var slot = orderManager.LobbyInfo.FirstEmptySlot();
                            var bot = currentMap.Rules.Actors["player"].TraitInfos<IBotInfo>().Select(t => t.Type).FirstOrDefault();

                            var botController = orderManager.LobbyInfo.Clients.FirstOrDefault(c => c.IsAdmin);

                            if (slot != null && bot != null)
                                orderManager.IssueOrder(Order.Command("slot_bot {0} {1} {2}".F(slot, botController.Index, bot)));
                            

                            addBotOnMapLoad = false;
                        }
                    });
                }).Start();
            }
                
        
        }

        void UpdatePlayerList() 
        {

            if (orderManager.LocalClient == null)
                return;

            var isHost = WarGame.IsHost;
            var idx = 0;

            foreach(var kv in orderManager.LobbyInfo.Slots){

                var key = kv.Key;
                var slot = kv.Value;
                var client = orderManager.LobbyInfo.ClientInSlot(key);


            }
        
        }

        void ConnectionStateChanged(OrderManager om)
        {
            if(om.Connection.ConnectionState == ConnectionState.NotConnected)
            {
                UI.CloseWindow();

                Action onConnect = () =>
                {
                    WarGame.OpenWindow("SERVER_LOBBY", new WidgetArgs()
                    {
                        { "onExit", onExit },
                        { "onStart", onStart },
                        { "skirmishMode", false }
                    });
                };
            }

        }
    }
}
using System;
using System.Collections.Generic;
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


        [ObjectCreator.UseCtor]
        internal LobbyLogic(Widget widget, ModData modData, WorldRenderer worldRenderer, OrderManager orderManager, Action onExit, Action onStart, bool skirmishMode)
        {
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

            var gameStarting = false;

            Action startGame = () =>
             {
                 gameStarting = true;
                 orderManager.IssueOrder(Order.Command("startgame"));
             };
        }

        void OnGameStart()
        {
            UI.CloseWindow();
            onStart();
        }

        void UpdateCurrentMap() { }

        void UpdatePlayerList() { }

        void ConnectionStateChanged(OrderManager om)
        {
            if(om.Connection.ConnectionState == ConnectionState.NotConnected)
            {
                UI.CloseWindow();
            }

        }
    }
}
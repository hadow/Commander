using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using EW.Primitives;
using EW.Widgets;
using EW.NetWork;
namespace EW.Mods.Common.Widgets.Logic
{
    public class MainMenuLogic:ChromeLogic
    {

        protected enum MenuType { Main,Singleplayer,Extras,MapEditor,SystemInfoPrompt,None}

        protected MenuType menuType = MenuType.Main;

        readonly Widget rootMenu;
        readonly LabelWidget newsStatus;


        [ObjectCreator.UseCtor]
        public MainMenuLogic(Widget widget,World world,ModData modData)
        {
            rootMenu = widget;
            rootMenu.Get<LabelWidget>("VERSION_LABEL").Text = modData.Manifest.Metadata.Version;

            //Menu buttons
            var mainMenu = widget.Get("MAIN_MENU");
            mainMenu.IsVisible = () => menuType == MenuType.Main;

            mainMenu.Get<ButtonWidget>("SINGLEPLAYER_BUTTON").OnClick = () => SwitchMenu(MenuType.Singleplayer);

            var singleplayerMenu = widget.Get("SINGLEPLAYER_MENU");
            singleplayerMenu.IsVisible = () => menuType == MenuType.Singleplayer;

            singleplayerMenu.Get<ButtonWidget>("SKIRMISH_BUTTON").OnClick = StartSkirmishGame;
            singleplayerMenu.Get<ButtonWidget>("BACK_BUTTON").OnClick = () => SwitchMenu(MenuType.Main);


            var missionsButton = singleplayerMenu.Get<ButtonWidget>("MISSIONS_BUTTON");
            missionsButton.OnClick = () =>
            {
                SwitchMenu(MenuType.None);
                WarGame.orderManager.IssueOrder(Order.Command("state {0}".F(Session.ClientState.NotReady)));
                WarGame.orderManager.IssueOrder(Order.Command("startgame"));
            };
        }

        void SwitchMenu(MenuType type)
        {
            menuType = type;
            
        }

        void StartSkirmishGame()
        {
            var map = WarGame.ModData.MapCache.ChooseInitialMap(WarGame.Settings.Server.Map, WarGame.CosmeticRandom);
            WarGame.Settings.Server.Map = map;

            ConnectionLogic.Connect(IPAddress.Loopback.ToString(),
                WarGame.CreateLocalServer(map), "", OpenSkirmishLobbyPanel, () => { WarGame.CloseServer();SwitchMenu(MenuType.Main); });
        }


        void OpenSkirmishLobbyPanel()
        {
            //SwitchMenu(MenuType.None);
            
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                WarGame.BeforeGameStart -= RemoveShellmapUI;
            }
            base.Dispose(disposing);
        }

        void RemoveShellmapUI()
        {
            rootMenu.Parent.RemoveChild(rootMenu);
        }
    }
}
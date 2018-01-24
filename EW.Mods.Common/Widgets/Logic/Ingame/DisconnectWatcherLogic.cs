using System;
using EW.Widgets;
using EW.NetWork;
namespace EW.Mods.Common.Widgets.Logic
{
    public class DisconnectWatcherLogic:ChromeLogic
    {
        [ObjectCreator.UseCtor]
        public DisconnectWatcherLogic(Widget widget,OrderManager orderManager)
        {
            var disconnected = false;

            widget.Get<LogicTickerWidget>("DISCONNECT_WATCHER").OnTick = ()=>{

                if (disconnected || orderManager.Connection.ConnectionState != ConnectionState.NotConnected)
                    return;

                //WarGame.RunAfterTick();

                disconnected = true;
            };

        }
    }
}

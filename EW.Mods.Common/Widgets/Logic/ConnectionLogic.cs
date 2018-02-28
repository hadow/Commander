using System;
using EW.Graphics;
using EW.NetWork;
using EW.Widgets;

namespace EW.Mods.Common.Widgets.Logic
{
    public class ConnectionLogic:ChromeLogic
    {

        Action onConnect, onAbort;
        Action<string> onRetry;

        [ObjectCreator.UseCtor]
        public ConnectionLogic(Widget widget,string host,int port,Action onConnect,Action onAbort,Action<string> onRetry)
        {
            this.onConnect = onConnect;
            this.onAbort = onAbort;
            this.onRetry = onRetry;

            WarGame.ConnectionStateChanged += ConnectionStateChanged;

            var panel = widget;
            panel.Get<ButtonWidget>("ABORT_BUTTON").OnClick = () => { CloseWindow(); onAbort(); };

            widget.Get<LabelWidget>("CONNECTING_DESC").GetText = () => "Connecting to {0}:{1}...".F(host, port);
        }


        void ConnectionStateChanged(OrderManager om)
        {
            if(om.Connection.ConnectionState == ConnectionState.Connected)
            {
                CloseWindow();
                onConnect();
            }
            else if(om.Connection.ConnectionState == ConnectionState.NotConnected)
            {
                CloseWindow();
                
            }
        }

        public static void Connect(string host,int port,string password,Action onConnect,Action onAbort)
        {
            WarGame.JoinServer(host, port, password);

            Action<string> onRetry = newPassword => Connect(host, port, newPassword, onConnect, onAbort);

            UI.OpenWindow("CONNECTING_PANEL", new WidgetArgs()
            {
                {"host",host },
                {"port",port },
                {"onConnect",onConnect },
                {"onAbort",onAbort },
                {"onRetry",onRetry },
            });
        }

        void CloseWindow()
        {
            WarGame.ConnectionStateChanged -= ConnectionStateChanged;
            UI.CloseWindow();
        }


    }


    public class ConnectionFailedLogic : ChromeLogic
    {
        [ObjectCreator.UseCtor]
        public ConnectionFailedLogic(Widget widget,OrderManager orderManager,Action onAbort,Action<string> onRetry)
        {
            var panel = widget;
            var abortButton = panel.Get<ButtonWidget>("ABORT_BUTTON");
            var retryButton = panel.Get<ButtonWidget>("RETRY_BUTTON");

            abortButton.Visible = onAbort != null;
            abortButton.OnClick = () => { UI.CloseWindow(); onAbort(); };
        }
    }
}
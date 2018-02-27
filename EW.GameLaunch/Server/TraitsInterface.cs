using System;
using System.Collections.Generic;
namespace EW.Server
{
    public interface INotifyServerStart { void ServerStarted(Server server); }


    public interface INotifyServerShutdown { void ServerShutdown(Server server); }

    public interface INotifyServerEmpty { void ServerEmpty(Server server); }

    public interface IStartGame { void GameStarted(Server server); }

    public interface IEndGame { void GameEnded(Server server); }

    public interface ITick
    {
        void Tick(Server server);

        int TickTimeout { get; }
    }
}
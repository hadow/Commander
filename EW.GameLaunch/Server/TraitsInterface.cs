using System;
using System.Collections.Generic;
using EW.NetWork;
namespace EW.Server
{
    public interface INotifyServerStart { void ServerStarted(Server server); }

    public interface INotifySyncLobbyInfo { void LobbyInfoSynced(Server server); }

    public interface INotifyServerShutdown { void ServerShutdown(Server server); }

    public interface INotifyServerEmpty { void ServerEmpty(Server server); }

    public interface IStartGame { void GameStarted(Server server); }

    public interface IEndGame { void GameEnded(Server server); }

    public interface ITick
    {
        void Tick(Server server);

        int TickTimeout { get; }
    }


    public interface IClientJoined { void ClientJoined(Server server, Connection conn); }

    public interface IInterpretCommand { bool InterpretCommand(Server server, Connection conn, Session.Client client, string cmd); }
}
using System;
using System.Collections.Generic;
using System.Linq;
using EW.Graphics;
using EW.NetWork;
using EW.Server;
using EW.Traits;
using S = EW.Server.Server;
namespace EW.Mods.Common.Server
{
    public class LobbyCommands:ServerTrait,INotifyServerStart,IInterpretCommand,INotifyServerEmpty,IClientJoined
    {

        public void ServerStarted(S server)
        {
            var uid = server.LobbyInfo.GlobalSettings.Map;
            server.Map = server.ModData.MapCache[uid];
            if (server.Map.Status != MapStatus.Available)
                throw new InvalidOperationException("Map {0} not found".F(uid));

            server.LobbyInfo.Slots = server.Map.Players.Players.
                Select(p => MakeSlotFromPlayerReference(p.Value))
                .Where(s => s != null)
                .ToDictionary(s => s.PlayerReference, s => s);

            LoadMapSettings(server,server.LobbyInfo.GlobalSettings,server.Map.Rules);
        }

        static bool ValidateSlotCommand(S server, Connection conn, Session.Client client, string arg, bool requiresHost)
        {

            if(!server.LobbyInfo.Slots.ContainsKey(arg)){
                return false;
            }

            if(requiresHost && !client.IsAdmin)
            {
                server.SendOrderTo(conn, "Message", "Only the host can do that");
                return false;
            }
            return true;
        }


        public static bool ValidateCommand(S server,EW.Server.Connection conn,Session.Client client,string cmd)
        {
            if (server.State == ServerState.GameStarted)
            {
                server.SendOrderTo(conn, "Message", "Cannot change state when game started.({0})".F(cmd));
                return false;
            }
            else if(client.State == Session.ClientState.Ready && !(cmd.StartsWith("state") || cmd == "startgame"))
            {
                server.SendOrderTo(conn, "Message", "Cannot change state when marked as ready.");
                return false;
            }

            return true;

        }


        void IClientJoined.ClientJoined(S server, EW.Server.Connection conn)
        {
            var client = server.GetClient(conn);


        }

        static void CheckAutoStart(S server)
        {
            var nonBotPlayers = server.LobbyInfo.NonBotPlayers;

            //Are all players and admin (could be spectating) ready?
            if (nonBotPlayers.Any(c => c.State != Session.ClientState.Ready) ||
                server.LobbyInfo.Clients.First(c => c.IsAdmin).State != Session.ClientState.Ready)
                return;

            //Does server have at least 2 human players?
            if (!server.LobbyInfo.GlobalSettings.EnableSinglePlayer && nonBotPlayers.Count() < 2)
                return;

            //Are the map conditions satisfied?
            if (server.LobbyInfo.Slots.Any(sl => sl.Value.Required && server.LobbyInfo.ClientInSlot(sl.Key) == null))
                return;

            server.StartGame();
        }


        public bool InterpretCommand(S server,EW.Server.Connection conn,Session.Client client,string cmd)
        {
            if (server == null || conn == null || client == null || !ValidateCommand(server, conn, client, cmd))
                return false;

            var dict = new Dictionary<string, Func<string, bool>>
            {
                {
                    "state",
                    s =>
                    {
                        var state = Session.ClientState.Invalid;
                        if(!Enum<Session.ClientState>.TryParse(s,false,out state))
                        {
                            server.SendOrderTo(conn,"Message","Malformed state command");
                            return true;
                        }
                        client.State = state;

                        server.SyncLobbyClients();
                        CheckAutoStart(server);
                        return true;
                    }
                },
                {
                    "startgame",
                    s =>
                    {
                        if (!client.IsAdmin)
                        {
                            server.SendOrderTo(conn,"Message","Only the host can start the game.");
                            return true;
                        }

                        if(server.LobbyInfo.Slots.Any(sl=>sl.Value.Required &&
                        server.LobbyInfo.ClientInSlot(sl.Key) == null))
                        {
                            server.SendOrderTo(conn,"Message","Unable to start the game until required slots are full.");
                            return true;
                        }

                        if(!server.LobbyInfo.GlobalSettings.EnableSinglePlayer && server.LobbyInfo.NonBotPlayers.Count() < 2)
                        {
                            server.SendOrderTo(conn,"Message",server.TwoHumansRequiredText);
                            return true;
                        }

                        server.StartGame();
                        return true;
                    }
                },
                {
                    "slot_bot",
                    s=>{

                        var parts = s.Split(' ');

                        if(parts.Length < 3){
                            server.SendOrderTo(conn,"Message"," Malformed slot_bot command");
                            return true;
                        }

                        if(!ValidateSlotCommand(server,conn,client,parts[0],true))
                            return false;

                        var slot  = server.LobbyInfo.Slots[parts[0]];
                        var bot = server.LobbyInfo.ClientInSlot(parts[0]);
                        int controllerClientIndex;

                        if(!Exts.TryParseIntegerInvariant(parts[1],out controllerClientIndex))
                            return false;
                        // Invalid slot

                        if(bot != null && bot.Bot == null)
                        {
                            server.SendOrderTo(conn,"Message","Can't add bots to a slot with  another client");
                            return true;
                        }

                        var botType = parts[2];
                        var botInfo = server.Map.Rules.Actors["player"].TraitInfos<IBotInfo>().FirstOrDefault(b=>b.Type == botType);

                        if(botInfo == null){
                            server.SendOrderTo(conn,"Message","Invalid bot type.");
                            return true;
                        }

                        slot.Closed  = false;
                        if(bot == null){

                            //Create a new bot
                            bot = new Session.Client(){

                                Index = server.ChooseFreePlayerIndex(),
                                Name = botInfo.Name,
                                Bot = botType,
                                Slot =parts[0],
                                Faction = "Random",
                                SpawnPoint =  0,
                                Team = 0,
                                State = Session.ClientState.NotReady,
                                BotControllerClientIndex = controllerClientIndex,
                            };

                            // Pick a random color for the bot
                            var validator = server.ModData.Manifest.Get<ColorValidator>();
                            var tileset = server.Map.Rules.TileSet;
                            var terrainColors = tileset.TerrainInfo.Where(ti=>ti.RestrictPlayerColor).Select(ti=>ti.Color);
                            var playerColors = server.LobbyInfo.Clients.Select(c=>c.Color.RGB)
                                                     .Concat(server.Map.Players.Players.Values.Select(p=>p.Color.RGB));

                            bot.Color = bot.PreferredColor = validator.RandomValidColor(server.Random, terrainColors, playerColors);

                            server.LobbyInfo.Clients.Add(bot);
                        }
                        else{

                            // Change the type of the existing bot
                            bot.Name = botInfo.Name;
                            bot.Bot = botType;
                        }

                        S.SyncClientToPlayerReference(bot,server.Map.Players.Players[parts[0]]);
                        server.SyncLobbyClients();
                        server.SyncLobbySlots();
                        return true;
                    }
                }
            };

            var cmdName = cmd.Split(' ').First();
            var cmdValue = cmd.Split(' ').Skip(1).JoinWith(" ");

            Func<string, bool> a;
            if (!dict.TryGetValue(cmdName, out a))
                return false;

            return a(cmdValue);
        }

        void INotifyServerEmpty.ServerEmpty(S server)
        {
            //Expire any temporary bans
            server.TempBans.Clear();

            //Re-enable spectators
            server.LobbyInfo.GlobalSettings.AllowSpectators = true;

            //Reset player slots
            server.LobbyInfo.Slots = server.Map.Players.Players.
                Select(p => MakeSlotFromPlayerReference(p.Value))
                .Where(ss => ss != null)
                .ToDictionary(ss => ss.PlayerReference, ss => ss);
        }


        static Session.Slot MakeSlotFromPlayerReference(PlayerReference pr)
        {
            if (!pr.Playable) return null;

            return new Session.Slot
            {
                PlayerReference = pr.Name,
                Closed = false,
                AllowBots = pr.AllowBots,
                LockFaction = pr.LockFaction,
                LockColor = pr.LockColor,
                LockTeam = pr.LockTeam,
                LockSpawn = pr.LockSpawn,
                Required = pr.Required,
            };
        }


        public static void LoadMapSettings(S server,Session.Global gs,Ruleset rules){

            var options = rules.Actors["player"].TraitInfos<ILobbyOptions>()
                               .Concat(rules.Actors["world"].TraitInfos<ILobbyOptions>())
                               .SelectMany(t => t.LobbyOptions(rules));

            foreach (var o in options)
            {
                var value = o.DefaultValue;
                var preferredValue = o.DefaultValue;
                Session.LobbyOptionState state;
                if (gs.LobbyOptions.TryGetValue(o.Id, out state))
                {
                    // Propagate old state on map change
                    if (!o.IsLocked)
                    {
                        if (o.Values.Keys.Contains(state.PreferredValue))
                            value = state.PreferredValue;
                        else if (o.Values.Keys.Contains(state.Value))
                            value = state.Value;
                    }

                    preferredValue = state.PreferredValue;
                }
                else
                    state = new Session.LobbyOptionState();

                state.IsLocked = o.IsLocked;
                state.Value = value;
                state.PreferredValue = preferredValue;
                gs.LobbyOptions[o.Id] = state;

                if (o.Id == "gamespeed")
                {
                    var speed = server.ModData.Manifest.Get<GameSpeeds>().Speeds[value];
                    gs.Timestep = speed.Timestep;
                    gs.OrderLatency = speed.OrderLatency;
                }
            }                  
        }
    }
}
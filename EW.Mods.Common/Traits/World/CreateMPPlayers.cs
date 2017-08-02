using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class CreateMPPlayersInfo : TraitInfo<CreateMPPlayers>
    {

    }
    /// <summary>
    /// 创建地图上的玩家
    /// </summary>
    public class CreateMPPlayers:ICreatePlayers
    {

        public void CreatePlayers(World w)
        {
            var players = new MapPlayers(w.Map.PlayerDefinitions).Players;
            var worldPlayers = new List<Player>();

            foreach(var kv in players.Where(p => !p.Value.Playable))
            {
                var player = new Player(w, null, kv.Value);
                worldPlayers.Add(player);

                if (kv.Value.OwnsWorld)
                    w.WorldActor.Owner = player;
            }
            
            Player localPlayer = null;

            //Create the regular playable players.


            worldPlayers.Add(new Player(w, null, new PlayerReference
            {
                Name = "Everyone",
                NonCombatant = true,
                Spectating = false,
                Faction = "Random",
                Allies = worldPlayers.Where(p=>!p.NonCombatant && p.Playable).Select(p=>p.InternalName).ToArray()
            }));

            w.SetPlayers(worldPlayers, localPlayer);
        }
    }
}
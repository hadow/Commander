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
            var worldOwnerFound = false;


            foreach(var kv in players.Where(p => !p.Value.Playable))
            {
                var player = new Player(w, null, kv.Value);
                worldPlayers.Add(player);

                if (kv.Value.OwnsWorld)
                {
                    worldOwnerFound = true;
                    w.SetWorldOwner(player);
                }
            }

            if (!worldOwnerFound)
                throw new InvalidOperationException("Map {0} does not define a player actor owning the world.".F(w.Map.Title));


            
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

            foreach(var p in w.Players){
                foreach(var q in w.Players){
                    if(!p.Stances.ContainsKey(q)){
                        p.Stances[q] = ChooseInitialStance(p, q);
                    }
                }
            }
        }


        static Stance ChooseInitialStance(Player p, Player q)
        {

            if (p == q)
                return Stance.Ally;


            if (q.Spectating && !p.NonCombatant && p.Playable)
                return Stance.Ally;

            if (p.PlayerReference.Allies.Contains(q.InternalName))
                return Stance.Ally;

            if (p.PlayerReference.Enemies.Contains(q.InternalName))
                return Stance.Enemy;




            return Stance.Neutral;
        }
    }


}
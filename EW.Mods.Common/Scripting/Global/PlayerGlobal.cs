using System;
using EW.Scripting;
using System.Linq;
using Eluant;
namespace EW.Mods.Common.Scripting
{
    [ScriptGlobal("Player")]
    public class PlayerGlobal:ScriptGlobal
    {


        public PlayerGlobal(ScriptContext context) : base(context) { }

        /// <summary>
        /// Returns the player with the specified internal name,or nil if a match is not found.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Player GetPlayer(string name)
        {
            return Context.World.Players.FirstOrDefault(p => p.InternalName == name);
        }

        public Player[] GetPlayers(LuaFunction filter)
        {
            return FilteredObjects(Context.World.Players, filter).ToArray();
        }
    }
}
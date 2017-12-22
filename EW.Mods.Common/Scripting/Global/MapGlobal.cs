using System;
using System.Collections.Generic;
using System.Linq;
using EW.Scripting;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.Scripting
{
    [ScriptGlobal("Map")]
    public class MapGlobal:ScriptGlobal
    {

        readonly SpawnMapActors sma;
        readonly World world;


        public MapGlobal(ScriptContext context) : base(context)
        {
            sma = context.World.WorldActor.Trait<SpawnMapActors>();
            world = context.World;

            //Register map actors as globals
            foreach (var kv in sma.Actors)
                context.RegisterMapActor(kv.Key, kv.Value);
        }


        /// <summary>
        /// Returns a table of all the actors that were specified in the map file.
        /// </summary>
        public Actor[] NamedActors
        {
            get
            {
                return sma.Actors.Values.ToArray();
            }
        }

        /// <summary>
        /// Returns the actor that was specified with a given name in the map file (or nil,if the actor is dead or not found).
        /// </summary>
        /// <param name="actorName"></param>
        /// <returns></returns>
        public Actor NamedActor(string actorName)
        {
            Actor ret;
            if (!sma.Actors.TryGetValue(actorName, out ret))
                return null;
            if (ret.Disposed)
                return null;

            return ret;
        }

        /// <summary>
        /// Returns the difficulty selected by the player before starting the mission.
        /// </summary>
        public string Difficulty
        {
            get
            {
                return string.Empty;
            }
        }



    }
}
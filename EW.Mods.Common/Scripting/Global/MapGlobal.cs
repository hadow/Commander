using System;
using System.Collections.Generic;
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
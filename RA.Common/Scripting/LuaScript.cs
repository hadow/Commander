using System;
using System.Collections.Generic;
using System.Linq;
using RA.Game;
using RA.Game.Graphics;
using RA.Game.Scripting;
namespace RA.Common.Scripting
{
    public class LuaScriptInfo:ITraitInfo
    {

        public readonly HashSet<string> Scripts = new HashSet<string>();
        public object Create()
        {
            return new LuaScript(this);
        }
    }

    /// <summary>
    /// Lua ½Å±¾
    /// </summary>
    public class LuaScript:ITick,IWorldLoaded
    {

        readonly LuaScriptInfo info;
        ScriptContext context;
        public LuaScript(LuaScriptInfo info)
        {
            this.info = info;
            
        }


        public void Tick(Actor actor)
        {
            context.Tick(actor);
        }



        public void WorldLoaded(World world,WorldRenderer render)
        {
            var scripts = info.Scripts ?? Enumerable.Empty<string>();
            context = new ScriptContext(world,render,scripts);
            context.WorldLoaded();

        }

    }
}
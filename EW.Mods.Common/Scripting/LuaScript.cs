using System;
using System.Collections.Generic;
using System.Linq;
using EW.Graphics;
using EW.Scripting;
using EW.Traits;
namespace EW.Mods.Common.Scripting
{
    public class LuaScriptInfo:ITraitInfo
    {

        public readonly HashSet<string> Scripts = new HashSet<string>();
        public object Create(ActorInitializer init)
        {
            return new LuaScript(this);
        }
    }

    /// <summary>
    /// Lua ½Å±¾
    /// </summary>
    public class LuaScript:ITick,IWorldLoaded,INotifyActorDisposing
    {

        readonly LuaScriptInfo info;
        ScriptContext context;

        bool disposed;
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
            context = new ScriptContext(world, render, scripts);
            context.WorldLoaded();

        }

        public void Disposing(Actor self)
        {
            if (disposed)
                return;
            if (context != null)
                context.Dispose();

            disposed = true;
        }

    }
}
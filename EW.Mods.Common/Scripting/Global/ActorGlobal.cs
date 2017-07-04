using System;
using Eluant;
using EW.Scripting;
using EW.Primitives;

namespace EW.Common.Scripting.Global
{
    [ScriptGlobal("Actor")]
    public class ActorGlobal:ScriptGlobal
    {
        public ActorGlobal(ScriptContext context) : base(context) { }

        public Actor Create(string type,bool addToWorld,LuaTable initTable)
        {
            var initDict = new TypeDictionary();

            //The actor must be added to the world at the end of the tick;
            var a = Context.World.CreateActor(false, type, initDict);
            if (addToWorld)
                Context.World.AddFrameEndTask(w => w.Add(a));

            return a;
        }

        public int BuildTime(string type,string queue = null)
        {
            ActorInfo ai;
            if (!Context.World.Map.Rules.Actors.TryGetValue(type, out ai))
                throw new LuaException("Unknown actor type '{0}'".F(type));
        }
    }
}
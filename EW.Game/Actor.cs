using System;
using System.Collections.Generic;
using Eluant;
using Eluant.ObjectBinding;
using RA.Scripting;
using RA.Activities;
using RA.Traits;
using RA.Primitives;
namespace RA
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Actor:IScriptBindable,IScriptNotifyBind, ILuaTableBinding,ILuaEqualityBinding,IEquatable<Actor>,IDisposable
    {

        public readonly ActorInfo Info;

        public readonly World World;

        public readonly uint ActorID;

        Activity currentActivity;


        internal Actor(World world,string name,TypeDictionary initDict)
        {
            var init = new ActorInitializer(this, initDict);
            
            World = world;
            ActorID = world.NextAID();
            
            if(name != null)
            {
                name = name.ToLowerInvariant();
            }
        }









        public void Tick()
        {
            currentActivity = ActivityUtils.RunActivity(this, currentActivity);
        }


        public bool Equals(Actor other)
        {
            return ActorID == other.ActorID;
        }

        public void Dispose()
        {

        }




        #region Scripting interface


        Lazy<ScriptActorInterface> luaInterface;
        public void OnScriptBind(ScriptContext context)
        {
            if (luaInterface == null)
                luaInterface = Exts.Lazy(() => new ScriptActorInterface(context, this));
        }

        public LuaValue this[LuaRuntime runtime,LuaValue keyValue]
        {
            get { return luaInterface.Value[runtime, keyValue]; }
            set { luaInterface.Value[runtime, keyValue] = value; }
        }

        public LuaValue Equals(LuaRuntime runtime,LuaValue left,LuaValue right)
        {
            Actor a, b;
            if(!left.TryGetClrValue(out a) || !right.TryGetClrValue(out b))
            {
                return false;
            }
            return a == b;
        }

        public LuaValue ToString(LuaRuntime runtime)
        {
            return "Actor ({0})".F(this);
        }

        public bool HasScriptProperty(string name)
        {
            return luaInterface.Value.ContainsKey(name);
        }


        #endregion

    }
}
using System;
using System.Collections.Generic;
using Eluant;
using Eluant.ObjectBinding;
namespace RA.Game.Scripting
{

    public interface IScriptBindable { }
    public abstract class ScriptObjectWrapper:IScriptBindable,ILuaTableBinding
    {

        Dictionary<string, ScriptMemberWrapper> members;

        protected readonly ScriptContext Context;


        public bool ContainsKey(string key) { return members.ContainsKey(key); }



        public ScriptObjectWrapper(ScriptContext context)
        {
            Context = context;
        }

        public LuaValue this[LuaRuntime runtime,LuaValue keyValue]
        {
            get
            {
                var name = keyValue.ToString();
                ScriptMemberWrapper wrapper;
                if (!members.TryGetValue(name, out wrapper))
                    throw new LuaException(MemberNotFoundError(name));

                return wrapper.Get(runtime);
            }
            set
            {
                var name = keyValue.ToString();
                ScriptMemberWrapper wrapper;
                if (!members.TryGetValue(name, out wrapper))
                    throw new LuaException(MemberNotFoundError(name));

                wrapper.Set(runtime, value);
            }
        }


        protected abstract string MemberNotFoundError(string memberName);
        protected abstract string DuplicateKeyError(string memberName);
    }
}
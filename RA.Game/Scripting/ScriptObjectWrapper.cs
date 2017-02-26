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

            }
        }


        protected abstract string MemberNotFoundError(string memberName);
    }
}
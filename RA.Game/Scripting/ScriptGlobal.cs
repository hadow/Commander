using System;
using System.Collections.Generic;


namespace RA.Game.Scripting
{
    public abstract class ScriptGlobal:ScriptObjectWrapper
    {
        protected override string MemberNotFoundError(string memberName)
        {
            throw new NotImplementedException();
        }
        
        public ScriptGlobal(ScriptContext context) : base(context)
        {

        }
    }
}
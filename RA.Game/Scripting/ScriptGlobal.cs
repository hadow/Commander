using System;
using System.Collections.Generic;
using System.Linq;

namespace RA.Scripting
{


    public sealed class ScriptGlobalAttribute : Attribute
    {
        public readonly string Name;
        public ScriptGlobalAttribute(string name) { Name = name; }
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class ScriptGlobal:ScriptObjectWrapper
    {
        public readonly string Name;

        protected override string MemberNotFoundError(string memberName)
        {
            throw new NotImplementedException();
        }

        protected override string DuplicateKeyError(string memberName)
        {
            throw new NotImplementedException();
        }

        public ScriptGlobal(ScriptContext context) : base(context)
        {
            var type = GetType();
            var names = type.GetCustomAttributes<ScriptGlobalAttribute>(true);
            if (names.Length != 1)
                throw new InvalidOperationException("[ScriptGlobal] attribute not found for global table '{0}'");

            Name = names.First().Name;

        }
    }
}
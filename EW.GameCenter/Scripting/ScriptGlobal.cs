using System;
using System.Collections.Generic;
using System.Linq;

namespace EW.Scripting
{


    public sealed class ScriptGlobalAttribute : Attribute
    {
        public readonly string Name;
        public ScriptGlobalAttribute(string name) { Name = name; }
    }

    /// <summary>
    /// Provides global bindings in lua code
    /// <remarks>
    ///     Instance methods and properties declared in derived classes will be made available in Lua.
    ///     Use<see cref="ScriptActorProperties"/> on your derived class to specify the name exposed in Lua.
    ///     
    ///     Any parameters to your method that are <see cref="LuaValue"/> will be disposed automatically when your method completes.
    ///     
    /// </remarks>
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
            Bind(new[] { this });
        }
    }
}
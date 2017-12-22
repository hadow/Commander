using System;
using System.Collections.Generic;
using System.Linq;
using Eluant;
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
            return "Table '{0}' does not define a property '{1}'".F(Name, memberName);
        }

        protected override string DuplicateKeyError(string memberName)
        {
            return "Table '{0}' defines multiple members '{1}".F(Name, memberName);
        }

        public ScriptGlobal(ScriptContext context) : base(context)
        {
            var type = GetType();
            var names = type.GetCustomAttributes<ScriptGlobalAttribute>(true);
            if (names.Length != 1)
                throw new InvalidOperationException("[ScriptGlobal] attribute not found for global table '{0}'".F(type));

            Name = names.First().Name;
            Bind(new[] { this });
        }

        protected IEnumerable<T> FilteredObjects<T>(IEnumerable<T> objects,LuaFunction filter)
        {

            if(filter != null)
            {
                objects = objects.Where(a =>
                {

                    using (var luaObject = a.ToLuaValue(Context))
                    using (var filterResult = filter.Call(luaObject))
                    using (var result = filterResult.First())
                        return result.ToBoolean();

                });
            }

            return objects;
        }
    }
}
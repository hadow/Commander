using System;
using System.Reflection;
using System.Collections.Generic;
namespace EW.Scripting
{
    public static class ScriptMemberExts
    {

        static readonly Dictionary<string, string> LuaTypeNameReplacements = new Dictionary<string, string>
        {
            {"Void","void" },
            {"Int32","int" },
            {"String","string" },
            {"Boolean","bool" }
        };

        public static string LuaDocString(this Type t)
        {
            string ret;
            if (!LuaTypeNameReplacements.TryGetValue(t.Name, out ret))
                ret = t.Name;
            return ret;
        }
        public static string LuaDocString(this ParameterInfo pi)
        {
            var ret = "{0} {1}".F(pi.ParameterType.LuaDocString(), pi.Name);
            if(pi.IsOptional)
                ret += " = {0}".F(pi.DefaultValue != null ? pi.DefaultValue : "nil");
            return ret;
        }

        public static string LuaDocString(this MemberInfo mi)
        {
            return string.Empty;
        }


    }
}
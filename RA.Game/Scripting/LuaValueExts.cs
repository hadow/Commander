using System;
using System.Collections.Generic;
using Eluant;
namespace RA.Game.Scripting
{

    /// <summary>
    /// ½Ó¿ÚÀ©Õ¹£¨only for LuaValue)
    /// </summary>
    public static class LuaValueExts
    {


        public static LuaValue ToLuaValue(this object obj,ScriptContext context)
        {

            throw new InvalidOperationException("Cannot convert type {0} to Lua,Class Must implement IScriptBindable.");
        }
    }

}
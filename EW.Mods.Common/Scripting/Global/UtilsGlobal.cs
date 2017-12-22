using System;
using System.Linq;
using Eluant;
using EW.Scripting;
using EW.Traits;
namespace EW.Mods.Common.Scripting
{
    [ScriptGlobal("Utils")]
    public class UtilsGlobal:ScriptGlobal
    {

        public UtilsGlobal(ScriptContext context) : base(context) { }



        /// <summary>
        /// Calls a function on every element in a collection.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="func"></param>
        public void Do(LuaValue[] collection,LuaFunction func)
        {
            foreach (var c in collection)
                func.Call(c).Dispose();
        }



        /// <summary>
        /// Returns a random integer x in the range low &lt;= x &lt; high
        /// </summary>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <returns></returns>
        public int RandomInteger(int low,int high)
        {
            if (high <= low)
                return low;
            return Context.World.SharedRandom.Next(low, high);
        }
    }
}
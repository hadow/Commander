using System;
using EW.Scripting;

namespace EW.Mods.Common.Scripting
{
    [ScriptGlobal("DateTime")]
    public class DateGlobal:ScriptGlobal
    {


        public DateGlobal(ScriptContext context) : base(context) { }

        /// <summary>
        /// Converts the number of seconds into game time(ticks)
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public int Seconds(int seconds)
        {
            return seconds * 25;
        }


        public int GameTime
        {
            get
            {
                return Context.World.WorldTick;
            }
        }


        public int Minutes(int minutes)
        {
            return Seconds(minutes * 60);
        }
    }
}
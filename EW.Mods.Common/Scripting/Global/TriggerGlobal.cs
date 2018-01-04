using System;
using EW.Scripting;
using EW.Effects;
using Eluant;
namespace EW.Mods.Common.Scripting
{
    [ScriptGlobal("Trigger")]
    public class TriggerGlobal:ScriptGlobal
    {
        public TriggerGlobal(ScriptContext context) : base(context) { }


        public static ScriptTriggers GetScriptTriggers(Actor a)
        {
            var events = a.TraitOrDefault<ScriptTriggers>();
            if (events == null)
                throw new LuaException("Actor '{0}' requires the ScriptTriggers trait before attaching a trigger".F(a.Info.Name));
            return events;
        }


        /// <summary>
        /// Call a function after a specified delay.The callback function will be called as func().
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="func"></param>
        public void AfterDelay(int delay,LuaFunction func)
        {
            var f = (LuaFunction)func.CopyReference();
            Action doCall = () =>
            {
                try
                {
                    using (f)
                        f.Call().Dispose();
                }
                catch (Exception e)
                {
                    Context.FatalError(e.Message);
                }
            };
            Context.World.AddFrameEndTask(w => w.Add(new DelayedAction(delay, doCall)));
        }


        public void OnProduction(Actor a,LuaFunction func)
        {
            GetScriptTriggers(a).RegisterCallback(Trigger.OnProduction, func, Context);
        }


        public void OnIdle(Actor a,LuaFunction func)
        {
            GetScriptTriggers(a).RegisterCallback(Trigger.OnIdle, func, Context);
        }


        public void OnDamaged(Actor a ,LuaFunction func)
        {
            GetScriptTriggers(a).RegisterCallback(Trigger.OnDamaged, func, Context);
        }
    }
}
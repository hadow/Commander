using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Scripting;
using Eluant;
namespace EW.Mods.Common.Scripting
{
    public enum Trigger
    {
        OnIdle,
        OnDamaged,
        OnKilled,
        OnProduction,OnOtherProduction,
        OnPlayerWon,OnPlayerLost,
        OnAddedToWorld,
        OnRemovedFromWorld,
        OnDiscovered,
        OnPlayerDiscovered,
        OnPassengerEntered,
        OnPassengerExited,
        OnSelling,
        OnSold,

    }
    public class ScriptTriggersInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new ScriptTriggers();
        }
    }
    public sealed class ScriptTriggers:INotifyActorDisposing
    {
        readonly List<Triggerable>[] triggerables = Exts.MakeArray(Enum.GetValues(typeof(Trigger)).Length, _ => new List<Triggerable>());
        struct Triggerable : IDisposable
        {
            public readonly LuaFunction Function;

            public readonly ScriptContext Context;

            public readonly LuaValue Self;

            public Triggerable(LuaFunction function,ScriptContext context,Actor self)
            {
                Function = (LuaFunction)function.CopyReference();
                Context = context;
                Self = self.ToLuaValue(Context);
            }

            public void Dispose()
            {
                Function.Dispose();
                Self.Dispose();
            }
        }


        public void Disposing(Actor self)
        {

        }


    }
}
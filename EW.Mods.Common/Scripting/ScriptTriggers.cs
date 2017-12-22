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
            return new ScriptTriggers(init.World,init.Self);
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

        readonly World world;
        readonly Actor self;


        public ScriptTriggers(World world,Actor self)
        {
            this.world = world;
            this.self = self;

        }


        public void RegisterCallback(Trigger trigger,LuaFunction func,ScriptContext context)
        {
            Triggerables(trigger).Add(new Triggerable(func, context, self));
        }

        List<Triggerable> Triggerables(Trigger trigger)
        {
            return triggerables[(int)trigger];
        }

        public void Clear(Trigger trigger)
        {
            world.AddFrameEndTask(w =>
            {
                var triggerables = Triggerables(trigger);
                foreach (var f in triggerables)
                    f.Dispose();

                triggerables.Clear();

            });
        }

        public void ClearAll()
        {
            foreach (Trigger t in Enum.GetValues(typeof(Trigger)))
                Clear(t);
        }
        public void Disposing(Actor self)
        {
            ClearAll();
        }


    }
}
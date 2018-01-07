using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Scripting;
using Eluant;
using EW.Mods.Common.Traits;
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
    public sealed class ScriptTriggers:INotifyActorDisposing,
    INotifyOtherProduction,
    INotifyProduction
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

        public event Action<Actor, Actor> OnOtherProducedInternal = (a, b) => { };
        public event Action<Actor, Actor> OnProducedInternal = (a, b) => { };



        public ScriptTriggers(World world,Actor self)
        {
            this.world = world;
            this.self = self;

        }

        public void UnitProducedByOther(Actor self,Actor producee,Actor produced,string productionType){

            if (world.Disposing)
                return;

            //Run Lua callbacks
            foreach(var f in Triggerables(Trigger.OnOtherProduction))
            {

                try{
                    using (var a = producee.ToLuaValue(f.Context))
                    using (var b = produced.ToLuaValue(f.Context))
                        f.Function.Call(a, b).Dispose();
                    
                }
                catch(Exception ex){
                    f.Context.FatalError(ex.Message);
                }
            }

            //Run any internally bound callbacks
            OnOtherProducedInternal(producee, produced);
        }

        public void UnitProduced(Actor self,Actor other,CPos exit){

            if (world.Disposing)
                return;
            
            //Run Lua callbacks
            foreach(var f in Triggerables(Trigger.OnProduction)){
                try{
                    using (var b = other.ToLuaValue(f.Context))
                        f.Function.Call(f.Self, b).Dispose();
                    
                }
                catch(Exception ex){
                    f.Context.FatalError(ex.Message);
                    return;
                }
            }

            //Run any internally bound callbacks
            OnProducedInternal(self, other);
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
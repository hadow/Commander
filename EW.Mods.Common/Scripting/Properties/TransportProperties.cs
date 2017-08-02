using System;
using System.Linq;
using EW.Scripting;
using EW.Traits;
using EW.Mods.Common.Traits;
using EW.Mods.Common.Activities;
namespace EW.Mods.Common.Scripting
{
    [ScriptPropertyGroup("Transports")]
    public class TransportProperties:ScriptActorProperties,Requires<CargoInfo>
    {
        readonly Cargo cargo;

        public TransportProperties(ScriptContext context,Actor self) : base(context, self)
        {
            cargo = self.Trait<Cargo>();
        }

        public void LoadPassenger(Actor a)
        {
            Console.WriteLine("LoadPassenger");
            cargo.Load(Self, a);
        }

        public Actor UnloadPassenger()
        {
            return cargo.Unload(Self);
        }

        public void UnloadPassengers()
        {
            Self.QueueActivity(new UnloadCargo());
        }
    }
}
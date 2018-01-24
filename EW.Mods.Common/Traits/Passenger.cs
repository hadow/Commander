using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class PassengerInfo : ITraitInfo
    {
        public readonly string CargoType = null;
        public readonly PipType PipType = PipType.Green;
        public readonly int Weight = 1;
        
        [VoiceReference]
        public readonly string Voice = "Action";
        public object Create(ActorInitializer init)
        {
            return new Passenger(this);
        }
    }
    public class Passenger:INotifyRemovedFromWorld
    {
        public readonly PassengerInfo Info;

        public Actor Transport;

        public Cargo ReservedCargo { get; private set; }
        public Passenger(PassengerInfo info)
        {
            Info = info;

        }

        void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
        {
            Unreserve(self);
        }

        public void Unreserve(Actor self)
        {
            if (ReservedCargo == null)
                return;
        }
    }
}
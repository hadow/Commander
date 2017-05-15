using System;
using System.Collections.Generic;

namespace EW.Mods.Common.Traits
{

    public class PassengerInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new Passenger();
        }
    }
    public class Passenger
    {
    }
}
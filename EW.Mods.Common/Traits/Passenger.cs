using System;
using System.Collections.Generic;
using EW.Traits;
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
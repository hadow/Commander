using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class PassengerInfo : ITraitInfo
    {
        public readonly string CargoType = null;
        public readonly int Weight = 1;

        [UpgradeGrantedReference]
        public readonly string[] GrantUpgrades = { };

        public object Create(ActorInitializer init)
        {
            return new Passenger(this);
        }
    }
    public class Passenger
    {
        public readonly PassengerInfo Info;

        public Actor Trasport;

        public Passenger(PassengerInfo info)
        {
            Info = info;

        }
    }
}
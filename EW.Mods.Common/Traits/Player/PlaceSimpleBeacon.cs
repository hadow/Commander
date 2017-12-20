using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class PlaceSimpleBeaconInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new PlaceSimpleBeacon();
        }
    }

    public class PlaceSimpleBeacon
    {
    }
}
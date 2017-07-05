using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class PlaceBeaconInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new PlaceBeacon(); }
    }
    class PlaceBeacon
    {
    }
}
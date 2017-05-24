using System;

namespace EW.Mods.Common.Traits
{
    public class RadarPingsInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new RadarPings();
        }
    }
    public class RadarPings
    {
    }
}
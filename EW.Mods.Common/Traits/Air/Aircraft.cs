using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class AircraftInfo : ITraitInfo
    {
        //巡航高度
        public readonly WDist CruiseAltitude = new WDist(1280);
        public readonly WDist IdealSeparation = new WDist(1706);

        public object Create(ActorInitializer init)
        {
            return new Aircraft();
        }
    }

    public class Aircraft
    {
    }
}
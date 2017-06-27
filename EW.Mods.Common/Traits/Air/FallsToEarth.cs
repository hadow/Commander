using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class FallsToEarthInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new FallsToEarth();
        }
    }

    class FallsToEarth
    {
    }
}
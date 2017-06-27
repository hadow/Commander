using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    class RenderDetectionCircleInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new RenderDetectionCircle();
        }
    }
    class RenderDetectionCircle
    {
    }
}
using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    class RenderDetectionCircleInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new RenderDetectionCircle(init.Self, this);
        }
    }
    class RenderDetectionCircle:ITick
    {
        public RenderDetectionCircle(Actor self,RenderDetectionCircleInfo info) { }
        public void Tick(Actor self) { }
    }
}
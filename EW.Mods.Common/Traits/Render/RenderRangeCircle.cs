using System;
using System.Collections.Generic;


namespace EW.Mods.Common.Traits
{
    class RenderRangeCircleInfo:ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new RenderRangeCircle();
        }
    }
    class RenderRangeCircle
    {
    }
}
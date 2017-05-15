using System;
using System.Collections.Generic;


namespace EW.Mods.Common.Traits
{


    public class DrawLineToTargetInfo : ITraitInfo
    {
        public virtual object Create(ActorInitializer init)
        {
            return new DrawLineToTarget();
        }
    }
    public class DrawLineToTarget
    {
    }
}
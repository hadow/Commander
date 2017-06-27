using System;
using System.Collections.Generic;

using EW.Traits;
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
using System;
using System.Collections.Generic;

namespace EW.Mods.Common.Traits
{

    public abstract class AffectsShroudInfo : ITraitInfo
    {
        public abstract object Create(ActorInitializer init);
    }

    public abstract class AffectsShroud
    {
    }
}
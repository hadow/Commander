using System;
using System.Collections.Generic;


namespace EW.Mods.Common.Traits
{

    public class MustBeDestroyedInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new MustBeDestroyed();
        }
    }

    public class MustBeDestroyed
    {
    }
}
using System;
using System.Collections.Generic;


namespace EW.Mods.Common.Traits
{

    class BurnsInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new Burns();
        }
    }
    class Burns
    {
    }
}
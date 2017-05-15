using System;
using System.Collections.Generic;


namespace EW.Mods.Common.Traits
{

    public class GuardInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new Guard();
        }
    }
    public class Guard
    {
    }
}
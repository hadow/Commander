using System;
using System.Collections.Generic;

namespace EW.Mods.Common.Traits
{


    public class ExplodesInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new Explodes();
        }
    }
    public class Explodes
    {
    }
}
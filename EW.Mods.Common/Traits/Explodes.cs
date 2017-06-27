using System;
using System.Collections.Generic;
using EW.Traits;
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
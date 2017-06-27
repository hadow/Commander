using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{

    class ScaredyCatInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new ScaredyCat(); }
    }
    class ScaredyCat
    {
    }
}
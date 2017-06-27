using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    class CrushableInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new Crushable(); }
    }
    class Crushable
    {
    }
}
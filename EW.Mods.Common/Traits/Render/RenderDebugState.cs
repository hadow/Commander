using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{

    class RenderDebugStateInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new RenderDebugState(); }
    }
    class RenderDebugState
    {
    }
}
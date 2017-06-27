using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class CombatDebugOverlayInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new CombatDebugOverlay();
        }
    }
    class CombatDebugOverlay
    {
    }
}
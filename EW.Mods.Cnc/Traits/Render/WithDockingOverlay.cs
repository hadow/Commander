using System;
using EW.Traits;

namespace EW.Mods.Cnc.Traits.Render
{
    public class WithDockingOverlayInfo:ITraitInfo
    {

        public object Create(ActorInitializer init)
        {
            return new WithDockingOverlay();
        }
    }

    public class WithDockingOverlay
    {

    }
}
using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class ExitsDebugOverlayInfo:ITraitInfo
    {

        object ITraitInfo.Create(ActorInitializer init)
        {
            return new ExitsDebugOverlay();
        }
    }

    public class ExitsDebugOverlay
    {

    }
}
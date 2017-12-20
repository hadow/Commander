using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class ExitsDebugOverlayManagerInfo:ITraitInfo
    {

        object ITraitInfo.Create(ActorInitializer init)
        {
            return new ExitsDebugOverlayManager();
        }

    }


    public class ExitsDebugOverlayManager
    {

    }
}
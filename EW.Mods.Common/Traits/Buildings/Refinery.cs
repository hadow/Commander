using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class RefineryInfo : Requires<WithSpriteBodyInfo>,IAcceptResourcesInfo
    {
        public object Create(ActorInitializer init) { return new Refinery(); }
    }
    public class Refinery
    {
    }


}
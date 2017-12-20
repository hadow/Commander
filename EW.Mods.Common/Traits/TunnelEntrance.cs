using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class TunnelEntranceInfo:ITraitInfo
    {
        public object Create(ActorInitializer init) { return new TunnelEntrance(); }

    }

    public class TunnelEntrance
    {

    }
}
using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    class BridgeInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new Bridge(init.Self, this); }
    }
    class Bridge
    {
        public Bridge(Actor self,BridgeInfo info)
        {

        }
    }
}
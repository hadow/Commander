using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    class BridgeHutInfo : ITraitInfo
    {

        public object Create(ActorInitializer init)
        {
            return new BridgeHut(init.World, this);
        }
    }
    class BridgeHut
    {

        public BridgeHut(World world,BridgeHutInfo info)
        {

        }
    }
}
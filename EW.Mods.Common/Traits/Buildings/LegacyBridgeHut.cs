using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{


    class LegacyBridgeHutInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new LegacyBridgeHut(init);
        }
    }
    class LegacyBridgeHut
    {


        public LegacyBridgeHut(ActorInitializer init)
        {

        }
    }
}
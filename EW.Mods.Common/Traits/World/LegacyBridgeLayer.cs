using System;
using EW.Traits;
using EW.Graphics;
namespace EW.Mods.Common.Traits
{

    class LegacyBridgeLayerInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new LegacyBridgeLayer(init.Self, this); }
    }
    class LegacyBridgeLayer:IWorldLoaded
    {

        public LegacyBridgeLayer(Actor self,LegacyBridgeLayerInfo info)
        {

        }


        public void WorldLoaded(World w,WorldRenderer wr)
        {

        }

    }
}
using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    class GroundLevelBridgeInfo:ITraitInfo
    {

        public object Create(ActorInitializer init)
        {
            return new GroundLevelBridge();
        }
    }

    class GroundLevelBridge
    {

    }
}
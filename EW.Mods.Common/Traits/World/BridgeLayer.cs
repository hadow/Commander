﻿using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    class BridgeLayerInfo : ITraitInfo
    {

        public object Create(ActorInitializer init)
        {
            return new BridgeLayer();
        }
    }


    class BridgeLayer
    {
    }
}
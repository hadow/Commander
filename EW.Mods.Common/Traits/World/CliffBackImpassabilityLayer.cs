using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    class CliffBackImpassabilityLayerInfo:ITraitInfo
    {

        public object Create(ActorInitializer init)
        {
            return new CliffBackImpassabilityLayer();
        }
    }


    class CliffBackImpassabilityLayer
    {

    }
}
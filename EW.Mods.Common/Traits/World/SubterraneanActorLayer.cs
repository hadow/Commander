using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class SubterraneanActorLayerInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new SubterraneanActorLayer();
        }
    }

    public class SubterraneanActorLayer
    {
    }
}
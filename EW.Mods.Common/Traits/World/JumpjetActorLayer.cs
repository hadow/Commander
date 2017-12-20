using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class JumpjetActorLayerInfo:ITraitInfo
    {

        public object Create(ActorInitializer init)
        {
            return new JumpjetActorLayer();
        }
    }


    public class JumpjetActorLayer
    {

    }
}
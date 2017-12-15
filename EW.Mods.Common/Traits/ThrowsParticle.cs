using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{


    class ThrowsParticleInfo : ITraitInfo,Requires<WithSpriteBodyInfo>,Requires<BodyOrientationInfo>
    {


        public object Create(ActorInitializer init)
        {
            return new ThrowsParticle(init, this);
        }

    }
    class ThrowsParticle
    {
        public ThrowsParticle(ActorInitializer init, ThrowsParticleInfo info)
        {

        }


    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace EW.Mods.Common.Traits.Sound
{


    class AmbientSoundInfo : ConditionalTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new AmbientSound(init.Self, this);
        }
    }


    class AmbientSound:ConditionalTrait<AmbientSoundInfo>
    {

        public AmbientSound(Actor self,AmbientSoundInfo info) : base(info)
        {

        }

    }
}
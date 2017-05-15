using System;
using System.Collections.Generic;


namespace EW.Mods.Common.Traits
{
    public class SoundOnDamageTransitionInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new SoundOnDamageTransition(); }
    }
    public class SoundOnDamageTransition
    {
    }
}
using System;
using System.Collections.Generic;

namespace EW.Mods.Common.Traits
{
    public class DeathSoundsInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new DeathSounds(); }
    }
    public class DeathSounds
    {
    }
}
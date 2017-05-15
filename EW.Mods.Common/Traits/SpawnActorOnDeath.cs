using System;
using System.Collections.Generic;

namespace EW.Mods.Common.Traits
{

    public class SpawnActorOnDeathInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new SpawnActorOnDeath(); }
    }
    public class SpawnActorOnDeath
    {
    }
}
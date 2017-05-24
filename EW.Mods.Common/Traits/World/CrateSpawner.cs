using System;

namespace EW.Mods.Common.Traits
{

    public class CrateSpawnerInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new CrateSpawner(); }
    }


    public class CrateSpawner
    {
    }
}
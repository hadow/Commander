using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class ShakeOnDeathInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new ShakeOnDeath(); }
    }
    public class ShakeOnDeath
    {
    }
}
using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class DeveloperModeInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new DeveloperMode(); }
    }
    class DeveloperMode
    {
    }
}
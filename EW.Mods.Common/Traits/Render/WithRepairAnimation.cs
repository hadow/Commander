using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class WithRepairAnimationInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new WithRepairAnimation(); }
    }
    public class WithRepairAnimation
    {
    }
}
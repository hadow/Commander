using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class ProvidesPrerequisiteInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new ProvidesPrerequisite(); }
    }
    public class ProvidesPrerequisite
    {
    }
}
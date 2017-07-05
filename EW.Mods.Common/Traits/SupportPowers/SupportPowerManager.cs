using System;

using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class SupportPowerManagerInfo : ITraitInfo, Requires<TechTreeInfo>
    {
        public object Create(ActorInitializer init) { return new SupportPowerManager(); }
    }
    public class SupportPowerManager
    {
    }
}
using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class GlobalUpgradeManagerInfo : ITraitInfo, Requires<TechTreeInfo>
    {
        public object Create(ActorInitializer init) { return new GlobalUpgradeManager(); }
    }
    class GlobalUpgradeManager
    {
    }
}
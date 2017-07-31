using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class CrateActionInfo : ITraitInfo
    {
        public virtual object Create(ActorInitializer init) { return new CrateAction(); }
    }
    public class CrateAction
    {
    }
}
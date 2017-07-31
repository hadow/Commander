using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class WithRoofInfo : ITraitInfo, Requires<RenderSpritesInfo>
    {
        public object Create(ActorInitializer init) { return new WithRoof(); }
    }
    public class WithRoof
    {
    }
}
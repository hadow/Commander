using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// This actor enables the radar minimap.
    /// </summary>
    public class ProvidesRadarInfo : ConditionalTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new ProvidesRadar(this);
        }
    }
    public class ProvidesRadar:ConditionalTrait<ProvidesRadarInfo>
    {
        public ProvidesRadar(ProvidesRadarInfo info) : base(info) { }
    }
}
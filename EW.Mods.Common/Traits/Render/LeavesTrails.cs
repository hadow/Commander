using System;
using EW.Traits;
namespace EW.Mods.Common.Traits.Render
{

    public class LeavesTrailsInfo : ConditionalTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new LeavesTrails(init.Self, this);
        }
    }
    public class LeavesTrails:ConditionalTrait<LeavesTrailsInfo>
    {

        public LeavesTrails(Actor self,LeavesTrailsInfo info) : base(info)
        {

        }
    }
}
using System;
using EW.Traits;
namespace EW.Mods.Common.Traits.Render
{
    public class WithVoxelBodyInfo:ConditionalTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new WithVoxelBody(init.Self, this);
        }
    }

    public class WithVoxelBody:ConditionalTrait<WithVoxelBodyInfo>
    {
        public WithVoxelBody(Actor self,WithVoxelBodyInfo info):base(info)
        {

        }
    }
}
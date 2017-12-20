using System;
using EW.Traits;
using EW.Graphics;


namespace EW.Mods.Common.Traits.Render
{
    public class WithVoxelBarrelInfo:ConditionalTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new WithVoxelBarrel(init.Self, this);
        }

    }


    public class WithVoxelBarrel : ConditionalTrait<WithVoxelBarrelInfo>
    {
        public WithVoxelBarrel(Actor self,WithVoxelBarrelInfo info) : base(info)
        {

        }
    }
}
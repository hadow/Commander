using System;
using EW.Traits;

namespace EW.Mods.Common.Traits.Render
{
    public class WithVoxelTurretInfo:ConditionalTraitInfo
    {

        public override object Create(ActorInitializer init)
        {
            return new WithVoxelTurret(init.Self, this);
        }
    }


    public class WithVoxelTurret : ConditionalTrait<WithVoxelTurretInfo>
    {
        public WithVoxelTurret(Actor self,WithVoxelTurretInfo info):base(info)
        {

        }
    }
}
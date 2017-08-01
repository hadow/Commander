using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class BlocksProjectilesInfo : UpgradableTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new BlocksProjectiles(init.Self, this);
        }
    }
    public class BlocksProjectiles:UpgradableTrait<BlocksProjectilesInfo>
    {
        public BlocksProjectiles(Actor self,BlocksProjectilesInfo info) : base(info) { }
    }
}
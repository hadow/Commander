using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class BlocksProjectilesInfo : UpgradableTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            throw new NotImplementedException();
        }
    }
    public class BlocksProjectiles:UpgradableTrait<BlocksProjectilesInfo>
    {
        public BlocksProjectiles(Actor self,BlocksProjectilesInfo info) : base(info) { }
    }
}
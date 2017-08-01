using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    class DamagedByTerrainInfo : UpgradableTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new DamagedByTerrain(init.Self, this);
        }
    }
    class DamagedByTerrain:UpgradableTrait<DamagedByTerrainInfo>
    {
        public DamagedByTerrain(Actor self,DamagedByTerrainInfo info) : base(info) { }
    }
}
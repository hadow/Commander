using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    class RepairsBridgesInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new RepairsBridges(this); }
    }
    class RepairsBridges
    {

        public RepairsBridges(RepairsBridgesInfo info)
        {

        }
    }
}
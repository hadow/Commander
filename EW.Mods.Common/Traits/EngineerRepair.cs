using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    class EngineerRepairInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new EngineerRepair(init,this); }
    }
    class EngineerRepair
    {
        public EngineerRepair(ActorInitializer init,EngineerRepairInfo info) { }
    }
}
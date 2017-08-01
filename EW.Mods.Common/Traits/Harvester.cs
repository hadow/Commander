using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class HarvesterInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new Harvester(init.Self, this); }
    }
    public class Harvester
    {

        public Harvester(Actor self,HarvesterInfo info)
        {

        }
    }
}
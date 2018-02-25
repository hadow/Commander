using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class PlayerExperienceInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new PlayerExperience(); }
    }
    public class PlayerExperience:ISync
    {

        [Sync]public int Experience { get; private set; }

        public void GiveExperience(int num)
        {
            Experience += num;
        }
    }
}
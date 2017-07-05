using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class UpdatesPlayerStatisticsInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new UpdatesPlayerStatistics();
        }
    }


    public class UpdatesPlayerStatistics
    {

    }

    public class PlayerStatisticsInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new PlayerStatistics(); }
    }
    class PlayerStatistics
    {
    }
}
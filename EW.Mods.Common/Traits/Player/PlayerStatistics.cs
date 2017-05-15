using System;


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
    class PlayerStatistics
    {
    }
}
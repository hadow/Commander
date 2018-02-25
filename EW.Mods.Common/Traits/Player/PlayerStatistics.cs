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


    public class UpdatesPlayerStatistics:INotifyKilled
    {

        void INotifyKilled.Killed(Actor self, AttackInfo attackInfo)
        {

        }

    }

    public class PlayerStatisticsInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new PlayerStatistics(); }
    }
    public class PlayerStatistics:ITick,IResolveOrder,INotifyCreated
    {
        PlayerResources resources;
        PlayerExperience experience;

        public int OrderCount;


        void INotifyCreated.Created(Actor self)
        {

        }

        void ITick.Tick(Actor self)
        {

        }

        void IResolveOrder.ResolveOrder(Actor self, NetWork.Order order)
        {

        }

    }
}
using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Mods.Common.Orders;
using EW.NetWork;
namespace EW.Mods.Common.Traits
{

    static class PrimaryExts
    {
        public static bool IsPrimaryBuilding(this Actor a)
        {
            var pb = a.TraitOrDefault<PrimaryBuilding>();
            return pb != null && pb.IsPrimary;
        }
    }
    [Desc("Used together with ClassicProductionQueue.")]
    public class PrimaryBuildingInfo:ITraitInfo
    {



        public object Create(ActorInitializer init)
        {
            return new PrimaryBuilding(init.Self,this);
        }

    }


    public class PrimaryBuilding:INotifyCreated,IIssueOrder,IResolveOrder
    {


        const string OrderID = "PrimaryProducer";

        readonly PrimaryBuildingInfo info;
        ConditionManager conditionManager;
        int primaryToken = ConditionManager.InvalidConditionToken;

        public bool IsPrimary { get; private set; }

        public PrimaryBuilding(Actor self, PrimaryBuildingInfo info)
        {
            this.info = info;
        }

        void INotifyCreated.Created(Actor self)
        {
            conditionManager = self.TraitOrDefault<ConditionManager>();
        }

        IEnumerable<IOrderTargeter> IIssueOrder.Orders
        {
            get { yield return new DeployOrderTargeter(OrderID, 1); }
        }


        Order IIssueOrder.IssueOrder(Actor self, IOrderTargeter order, Target target, bool queued)
        {
            if (order.OrderID == OrderID)
                return new Order(order.OrderID, self, false);

            return null;
        }

        void IResolveOrder.ResolveOrder(Actor self, Order order)
        {
            //var forceRallyPoint = RallyPoint.IsForceSet(order);
            //if (order.OrderString == OrderID || forceRallyPoint)
                //SetPrimaryProducer(self, !IsPrimary || forceRallyPoint);
        }
    }
}
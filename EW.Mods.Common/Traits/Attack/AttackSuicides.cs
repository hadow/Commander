using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Mods.Common.Orders;
namespace EW.Mods.Common.Traits
{
    class AttackSuicidesInfo : ITraitInfo, Requires<IMoveInfo>
    {
        [VoiceReference]
        public readonly string Voice = "Action";

        public object Create(ActorInitializer init) { return new AttackSuicides(init.Self,this); }

    }

    class AttackSuicides:IIssueOrder,IResolveOrder,IOrderVoice
    {
        readonly AttackSuicidesInfo info;
        readonly IMove move;
        public AttackSuicides(Actor self,AttackSuicidesInfo info)
        {
            this.info = info;
            this.move = self.Trait<IMove>();
        }


        public IEnumerable<IOrderTargeter> Orders
        {
            get
            {
                yield return new DeployOrderTargeter("Detonate", 5);
            }
        }

        public Order IssueOrder(Actor self,IOrderTargeter order,Target target,bool queued)
        {
            if (order.OrderID != "DetonateAttack" && order.OrderID != "Detonate")
                return null;
            if (target.Type == TargetT.FrozenActor)
                return new Order(order.OrderID, self, queued) { ExtraData = target.FrozenActor.ID };

            return new Order(order.OrderID, self, queued) { TargetActor = target.Actor };
        }

        public void ResolveOrder(Actor self,Order order)
        {

        }

        public string VoicePhraseForOrder(Actor self,Order order)
        {
            return info.Voice;
        }
    }
}
using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Mods.Common.Orders;
using EW.NetWork;
using EW.Activities;
using System.Drawing;
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
            //if (target.Type == TargetT.FrozenActor)
            //    return new Order(order.OrderID, self, queued) { ExtraData = target.FrozenActor.ID };

            //return new Order(order.OrderID, self, queued) { TargetActor = target.Actor };
            return new Order(order.OrderID, self, target, queued);
        }

        public void ResolveOrder(Actor self,Order order)
        {
            if(order.OrderString == "DetonateAttack")
            {
                var target = self.ResolveFrozenActorOrder(order, Color.Red);
                if (target.Type != TargetT.Actor)
                    return;

                if (!order.Queued)
                    self.CancelActivity();

                self.SetTargetLine(target, Color.Red);

                self.QueueActivity(move.MoveToTarget(self, target));

                self.QueueActivity(new CallFunc(() => self.Kill(self)));
            }
        }

        public string VoicePhraseForOrder(Actor self,Order order)
        {
            return info.Voice;
        }
    }
}
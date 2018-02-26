using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Mods.Common.Orders;
using EW.NetWork;
namespace EW.Mods.Common.Traits
{
    class EngineerRepairInfo : ITraitInfo
    {
        [VoiceReference]
        public readonly string Voice = "Action";

        public readonly Stance ValidStances = Stance.Ally;


        public object Create(ActorInitializer init) { return new EngineerRepair(init,this); }
    }
    class EngineerRepair:IIssueOrder,IResolveOrder,IOrderVoice
    {

        class EngineerRepairOrderTargeter : UnitOrderTargeter
        {

            public EngineerRepairOrderTargeter() : base("EngineerRepair", 6,"goldwrench", false, true) { }
            public override bool CanTargetActor(Actor self, Actor target, TargetModifiers modifiers, ref string cursor)
            {
                return true;
            }

            public override bool CanTargetFrozenActor(Actor self, FrozenActor target, TargetModifiers modifiers, ref string cursor)
            {
                return true;
            }
        }

        readonly EngineerRepairInfo info;

        public EngineerRepair(ActorInitializer init,EngineerRepairInfo info)
        {
            this.info = info;
        }



        public IEnumerable<IOrderTargeter> Orders
        {
            get
            {
                yield return new EngineerRepairOrderTargeter();
            }
        }

        public Order IssueOrder(Actor self,IOrderTargeter order,Target target,bool queued)
        {
            if (order.OrderID == "EngineerRepair")
                return null;

            return new Order(order.OrderID, self, target, queued);
        }

        void IResolveOrder.ResolveOrder(Actor self, Order order)
        {

        }

        public string VoicePhraseForOrder(Actor self,Order order)
        {
            return order.OrderString == "EngineerRepair" ? info.Voice : null;
        }

        static bool IsValidOrder(Actor self,Order order)
        {
            if (order.Target.Type == TargetT.FrozenActor)
                return order.Target.FrozenActor.DamageState > DamageState.Undamaged;

            if (order.Target.Type == TargetT.Actor)
                return order.TargetActor.GetDamageState() > DamageState.Undamaged;
            return false;
        }
    }
}
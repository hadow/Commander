using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Framework;
using EW.Mods.Common.Activities;
using System.Drawing;
namespace EW.Mods.Common.Traits
{

    class AttackMoveInfo : ITraitInfo,Requires<IMoveInfo>
    {
        [VoiceReference]
        public readonly string Voice = "Action";

        public object Create(ActorInitializer init)
        {
            return new AttackMove(init.Self,this);
        }

    }
    class AttackMove:IResolveOrder,IOrderVoice,INotifyIdle,ISync
    {
        readonly IMove move;
        readonly AttackMoveInfo info;

        [Sync]
        public CPos _targetLocation
        {
            get
            {
                return TargetLocation.HasValue ? TargetLocation.Value : CPos.Zero;
            }
        }
        public CPos? TargetLocation = null;
        public AttackMove(Actor self,AttackMoveInfo info)
        {
            move = self.Trait<IMove>();
            this.info = info;
        }


        public string VoicePhraseForOrder(Actor self,Order order)
        {
            if (order.OrderString == "AttackMove")
                return info.Voice;
            return null;
        }


        public void ResolveOrder(Actor self,Order order)
        {
            TargetLocation = null;

            if (order.OrderString == "AttackMove")
            {
                TargetLocation = move.NearestMoveableCell(order.TargetLocation);
                self.SetTargetLine(Target.FromCell(self.World, TargetLocation.Value), Color.Red);
                Activate(self);
            }
        }

        public void TickIdle(Actor self)
        {
            if (TargetLocation.HasValue && self.Location != TargetLocation.Value)
                Activate(self);
        }

        void Activate(Actor self)
        {
            self.CancelActivity();
            self.QueueActivity(new AttackMoveActivity(self, move.MoveTo(TargetLocation.Value,1)));
        }
    }
}
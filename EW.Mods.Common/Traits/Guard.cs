using System;
using System.Collections.Generic;
using System.Drawing;
using EW.Traits;
using EW.NetWork;
using EW.Mods.Common.Activities;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// The Player can give this unit the order to follow and protect friendly units with the Guardable trait.
    /// </summary>
    public class GuardInfo : ITraitInfo
    {
        [VoiceReference]
        public readonly string Voice = "Action";

        public object Create(ActorInitializer init)
        {
            return new Guard(this);
        }
    }
    public class Guard:IResolveOrder,IOrderVoice,INotifyCreated
    {

        readonly GuardInfo info;
        IMove move;


        public Guard(GuardInfo info)
        {
            this.info = info;
        }


        void INotifyCreated.Created(Actor self)
        {
            move = self.Trait<IMove>();
        }


        public void ResolveOrder(Actor self,Order order)
        {
            if (order.OrderString == "Guard")
                GuardTarget(self, Target.FromActor(order.Target.SerializableActor), order.Queued);
        }


        public void GuardTarget(Actor self,Target target,bool queued = false)
        {
            if (!queued)
                self.CancelActivity();

            self.SetTargetLine(target, Color.Yellow);

            var range = target.Actor.Info.TraitInfo<GuardableInfo>().Range;
            self.QueueActivity(new AttackMoveActivity(self, move.MoveFollow(self, target, WDist.Zero, range)));
        }

        public string VoicePhraseForOrder(Actor self,Order order)
        {
            return order.OrderString == "Guard" ? info.Voice : null;
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using EW.Traits;
using EW.Mods.Common.Orders;
using EW.NetWork;
namespace EW.Mods.Common.Traits
{
    [Desc("This actor can interact with TunnelEntrances to move through TerrainTunnels.")]
    public class EntersTunnelsInfo:ITraitInfo
    {

        public readonly string EnterCursor = "enter";
        public readonly string EnterBlockedCursor = "enter-blocked";

        [VoiceReference] public readonly string Voice = "Action";

        public object Create(ActorInitializer init) { return new EntersTunnels(init.Self,this); }
    }


    public class EntersTunnels:IIssueOrder,IResolveOrder,IOrderVoice
    {


        readonly EntersTunnelsInfo info;
        readonly IMove move;

        public EntersTunnels(Actor self,EntersTunnelsInfo info){

            this.info = info;
            move = self.Trait<IMove>();
        }

        public Order IssueOrder(Actor self, IOrderTargeter order, Target target, bool queued)
        {
            if (order.OrderID != "EnterTunnel")
                return null;

            return new Order(order.OrderID, self, target, queued) { SuppressVisualFeedback = true };
        }

        public IEnumerable<IOrderTargeter> Orders
        {
            get
            {
                yield return new EnterTunnelOrderTargeter(info);
            }
        }

        public string VoicePhraseForOrder(Actor self, Order order)
        {
            return order.OrderString == "EnterTunnel" ? info.Voice : null;
        }


        void IResolveOrder.ResolveOrder(Actor self, Order order){


            if (order.OrderString != "EnterTunnel")
                return;

            var target = self.ResolveFrozenActorOrder(order, Color.Red);
            if (target.Type != TargetT.Actor)
                return;

            var tunnel = target.Actor.TraitOrDefault<TunnelEntrance>();
            if (!tunnel.Exit.HasValue)
                return;

            if (!order.Queued)
                self.CancelActivity();

            self.SetTargetLine(Target.FromCell(self.World, tunnel.Exit.Value), Color.Green);
            self.QueueActivity(move.MoveTo(tunnel.Entrance, tunnel.NearEnough));
            self.QueueActivity(move.MoveTo(tunnel.Exit.Value, tunnel.NearEnough));


        }

        class EnterTunnelOrderTargeter:UnitOrderTargeter{

            readonly EntersTunnelsInfo info;

            public EnterTunnelOrderTargeter(EntersTunnelsInfo info):base("EnterTunnel",6,info.EnterCursor,true,true){

                this.info = info;
            }

			public override bool CanTargetActor(Actor self, Actor target, TargetModifiers modifiers, ref string cursor)
			{
                if (target == null || target.IsDead)
                    return false;

                var tunnel = target.TraitOrDefault<TunnelEntrance>();
                if (tunnel == null)
                    return false;

                var buildingInfo = target.Info.TraitInfoOrDefault<BuildingInfo>();
                if(buildingInfo!=null){

                    var footprint = buildingInfo.PathableTiles(target.Location);
                    if(footprint.All(c=>self.World.ShroudObscures(c))){
                        return false;
                    }
                }

                if(!tunnel.Exit.HasValue){
                    cursor = info.EnterBlockedCursor;
                    return false;
                }

                cursor = info.EnterCursor;
                return true;
            }


			public override bool CanTargetFrozenActor(Actor self, FrozenActor target, TargetModifiers modifiers, ref string cursor)
			{
                return CanTargetActor(self, target.Actor, modifiers, ref cursor);
			}
		}

    }
}
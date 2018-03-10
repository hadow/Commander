using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.NetWork;
using EW.Mods.Common.Activities;
using System.Drawing;
using EW.Orders;
using EW.Framework.Touch;
namespace EW.Mods.Common.Traits
{

    class AttackMoveInfo : ITraitInfo,Requires<IMoveInfo>
    {

        [GrantedConditionReference]
        [Desc("The condition to grant to self while scanning for targets during an attack-move.")]
        public readonly string AttackMoveScanCondition = null;

        [GrantedConditionReference]
        [Desc("The condition to grant to self while scanning for targets during an assault-move.")]
        public readonly string AssaultMoveScanCondition = null;

        [Desc("Can the actor be ordered to move in to shroud?")]
        public readonly bool MoveIntoShroud = true;

        [VoiceReference]
        public readonly string Voice = "Action";

        public object Create(ActorInitializer init)
        {
            return new AttackMove(init.Self,this);
        }

    }
    class AttackMove:IResolveOrder,IOrderVoice,INotifyIdle,ISync,INotifyCreated,ITick
    {
        ConditionManager conditionManager;

        readonly IMove move;
        public readonly AttackMoveInfo Info;

        [Sync]
        public CPos _targetLocation
        {
            get
            {
                return TargetLocation.HasValue ? TargetLocation.Value : CPos.Zero;
            }
        }
        public CPos? TargetLocation = null;

        int attackMoveToken = ConditionManager.InvalidConditionToken;
        int assaultMoveToken = ConditionManager.InvalidConditionToken;
        bool assaultMoving = false;

        public AttackMove(Actor self,AttackMoveInfo info)
        {
            move = self.Trait<IMove>();
            this.Info = info;
        }


        public string VoicePhraseForOrder(Actor self,Order order)
        {

            if (!Info.MoveIntoShroud && !self.Owner.Shroud.IsExplored(order.TargetLocation))
                return null;
            
            if (order.OrderString == "AttackMove")
                return Info.Voice;
            return null;
        }


        public void ResolveOrder(Actor self,Order order)
        {
            TargetLocation = null;

            if (order.OrderString == "AttackMove" || order.OrderString == "AssaultMove")
            {

                if (!order.Queued)
                    self.CancelActivity();

                if (!Info.MoveIntoShroud && !self.Owner.Shroud.IsExplored(order.TargetLocation))
                    return;
                
                TargetLocation = move.NearestMoveableCell(order.TargetLocation);
                self.SetTargetLine(Target.FromCell(self.World, TargetLocation.Value), Color.Red);
                Activate(self,order.OrderString == "AssaultMove");
            }
        }

        void INotifyIdle.TickIdle(Actor self)
        {
            // This might cause the actor to be stuck if the target location is unreachable

            if (TargetLocation.HasValue && self.Location != TargetLocation.Value)
                Activate(self,assaultMoving);
        }

        void Activate(Actor self,bool assaultMove)
        {
            assaultMoving = assaultMove;
            self.QueueActivity(new AttackMoveActivity(self, move.MoveTo(TargetLocation.Value,1)));
        }


        void INotifyCreated.Created(Actor self){

            conditionManager = self.TraitOrDefault<ConditionManager>();


        }

        void ITick.Tick(Actor self){


            if (conditionManager == null)
                return;

            var activity = self.CurrentActivity as AttackMoveActivity;
            var attackActive = activity != null && !assaultMoving;
            var assaultActive = activity != null && assaultMoving;

            if (attackActive && attackMoveToken == ConditionManager.InvalidConditionToken && !string.IsNullOrEmpty(Info.AttackMoveScanCondition))
                attackMoveToken = conditionManager.GrantCondition(self, Info.AttackMoveScanCondition);
            else if (!attackActive && attackMoveToken != ConditionManager.InvalidConditionToken)
                attackMoveToken = conditionManager.RevokeCondition(self, attackMoveToken);
                
            if (assaultActive && assaultMoveToken == ConditionManager.InvalidConditionToken && !string.IsNullOrEmpty(Info.AssaultMoveScanCondition))
                assaultMoveToken = conditionManager.GrantCondition(self, Info.AssaultMoveScanCondition);
            else if (!assaultActive && assaultMoveToken != ConditionManager.InvalidConditionToken)
                assaultMoveToken = conditionManager.RevokeCondition(self, assaultMoveToken);
        }


    }

    public class AttackMoveOrderGenerator:UnitOrderGenerator
    {


        readonly TraitPair<AttackMove>[] subjects;

        readonly GestureType expectedGestureType;

        public AttackMoveOrderGenerator(IEnumerable<Actor> subjects,GestureType type)
        {

            expectedGestureType = type;

            this.subjects = subjects.Where(a => !a.IsDead)
                .SelectMany(a => a.TraitsImplementing<AttackMove>()
                            .Select(am => new TraitPair<AttackMove>(a, am))).ToArray();


        }

        public override IEnumerable<Order> Order(World world, CPos cell, Framework.Int2 worldPixel, GestureSample gs)
        {
            if (gs.GestureType != expectedGestureType)
                world.CancelInputMode();

            return OrderInner(world, cell, gs);
        }


        protected virtual IEnumerable<Order> OrderInner(World world,CPos cell,GestureSample gs){

                if(gs.GestureType == expectedGestureType)
                {
                    world.CancelInputMode();

                var queued = false;
                bool assaultOrAttack_Move = true;
                var orderName = assaultOrAttack_Move ? "AssaultMove" : "AttackMove";

                // Cells outside the playable area should be clamped to the edge for consistency with move orders
                cell = world.Map.Clamp(cell);
                foreach (var s in subjects)
                    yield return new Order(orderName, s.Actor, Target.FromCell(world, cell), queued);
                }
                
            }


    }


}
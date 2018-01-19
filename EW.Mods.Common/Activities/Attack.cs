using System;
using System.Linq;
using EW.Activities;
using EW.Traits;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.Activities
{
	public class Attack:Activity
	{
        [Flags]
        protected enum AttackStatus
        {
            UnableToAttack,
            NeedsToTurn,
            NeedsToMove,
            Attacking
        }

        protected readonly Target Target;
        readonly AttackBase[] attackTraits;
        readonly IMove move;
        readonly IFacing facing;
        readonly IPositionable positionable;

        readonly bool forceAttack;
        readonly int facingTolerance;

        WDist minRange;
        WDist maxRange;
        Activity turnActivity;
        Activity moveActivity;
        AttackStatus attackStatus = AttackStatus.UnableToAttack;

		public Attack(Actor self,Target target,bool allowMovement,bool forceAttack,int facingTolerance)
		{
            Target = target;

            this.forceAttack = forceAttack;
            this.facingTolerance = facingTolerance;

            attackTraits = self.TraitsImplementing<AttackBase>().ToArray();
            facing = self.Trait<IFacing>();
            positionable = self.Trait<IPositionable>();

            move = allowMovement ? self.TraitOrDefault<IMove>() : null;
		}

        public override Activity Tick(Actor self)
        {
            turnActivity = moveActivity = null;
            attackStatus = AttackStatus.UnableToAttack;

            foreach(var attack in attackTraits.Where(x => !x.IsTraitDisabled))
            {
                var status = TickAttack(self, attack);
                attack.IsAniming = status == AttackStatus.Attacking || status == AttackStatus.NeedsToTurn;
            }

            if (attackStatus.HasFlag(AttackStatus.Attacking))
                return this;

            if (attackStatus.HasFlag(AttackStatus.NeedsToTurn))
                return turnActivity;

            if (attackStatus.HasFlag(AttackStatus.NeedsToMove))
                return moveActivity;

            return NextActivity;
        }

        protected virtual AttackStatus TickAttack(Actor self,AttackBase attack)
        {
            if (IsCanceled)
                return AttackStatus.UnableToAttack;

            var type = Target.Type;
            if (!Target.IsValidFor(self) || type == TargetT.FrozenActor)
                return AttackStatus.UnableToAttack;

            if (attack.Info.AttackRequiresEnteringCell && !positionable.CanEnterCell(Target.Actor.Location, null, false))
                return AttackStatus.UnableToAttack;

            //Drop the target if it moves under the shroud / fog.
            if (!attack.Info.IgnoresVisibility && type == TargetT.Actor
                && !Target.Actor.Info.HasTraitInfo<FrozenUnderFogInfo>()
                && !Target.Actor.CanBeViewedByPlayer(self.Owner))
                return AttackStatus.UnableToAttack;

            //Drop the target once none of the weapons are effective against it

            var armaments = attack.ChooseArmamentsForTarget(Target, forceAttack).ToList();
            if (armaments.Count == 0)
                return AttackStatus.UnableToAttack;

            //Update ranges
            minRange = armaments.Max(a => a.Weapon.MinRange);
            maxRange = armaments.Min(a => a.MaxRange());

            var pos = self.CenterPosition;
            var mobile = move as Mobile;
            if(!Target.IsInRange(pos,maxRange) 
                || (minRange.Length != 0 && Target.IsInRange(pos,minRange))
                || (mobile != null && !mobile.CanInteractWithGroundLayer(self)))
            {
                //Try to move within range,drop the target otherwise.
                if (move == null)
                    return AttackStatus.UnableToAttack;

                attackStatus |= AttackStatus.NeedsToMove;
                moveActivity = ActivityUtils.SequenceActivities(move.MoveWithinRange(Target, minRange, maxRange), this);
                return AttackStatus.NeedsToMove;
            }

            var targetedPosition = attack.GetTargetPosition(pos, Target);
            var desiredFacing = (targetedPosition - pos).Yaw.Facing;

            if(!Util.FacingWithinTolerance(facing.Facing,desiredFacing,facingTolerance))
            {
                attackStatus |= AttackStatus.NeedsToTurn;
                turnActivity = ActivityUtils.SequenceActivities(new Turn(self, desiredFacing), this);
                return AttackStatus.NeedsToTurn;
            }

            attackStatus |= AttackStatus.Attacking;
            attack.DoAttack(self, Target, armaments);

            return AttackStatus.Attacking;

        }
    }
}

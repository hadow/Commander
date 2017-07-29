using System;
using EW.Activities;
using EW.Traits;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.Activities
{
	public class Attack:Activity
	{
        enum AttackStatus
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

		public Attack(Actor self,Target target,bool allowMovement,bool forceAttack)
		{
			
		}

        public override Activity Tick(Actor self)
        {
            throw new NotImplementedException();
        }
    }
}

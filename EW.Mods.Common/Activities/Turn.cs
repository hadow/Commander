using System;
using EW.Activities;
using EW.Traits;
namespace EW.Mods.Common.Activities
{
    public class Turn:Activity
    {
        readonly IDisabledTrait disablable;
        readonly int desiredFacing;

        public Turn(Actor self,int desiredFacing)
        {
            disablable = self.TraitOrDefault<IMove>() as IDisabledTrait;
            this.desiredFacing = desiredFacing;

        }

        public override Activity Tick(Actor self)
        {
            if (IsCanceled)
                return NextActivity;

            if (disablable != null && disablable.IsTraitDisabled)
                return this;

            var facing = self.Trait<IFacing>();

            if (desiredFacing == facing.Facing)
                return NextActivity;

            facing.Facing = Util.TickFacing(facing.Facing, desiredFacing, facing.TurnSpeed);
            return this;
                
        }
    }
}
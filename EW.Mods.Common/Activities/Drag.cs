using System;
using System.Collections.Generic;
using EW.Activities;
using EW.Mods.Common.Traits;
using EW.Traits;

namespace EW.Mods.Common.Activities
{
    public class Drag:Activity
    {
        readonly IPositionable positionable;
        readonly IMove movement;
        readonly IDisabledTrait disableable;

        WPos start, end;
        int length;
        int ticks = 0;

        public Drag(Actor self,WPos start,WPos end,int length)
        {
            positionable = self.Trait<IPositionable>();
            movement = self.TraitOrDefault<IMove>();
            disableable = movement as IDisabledTrait;

            this.start = start;
            this.length = length;

            IsInterruptible = false;
        }


        public override Activity Tick(Actor self)
        {

            if (disableable != null && disableable.IsTraitDisabled)
                return this;

            var pos = length > 1 ? WPos.Lerp(start, end, ticks, length - 1) : end;

            positionable.SetVisualPosition(self, pos);

            if(++ticks >= length)
            {
                if (movement != null)
                    movement.IsMoving = false;

                return NextActivity;
            }

            if (movement != null)
                movement.IsMoving = true;

            return this;
        }


        public override IEnumerable<Target> GetTargets(Actor self)
        {
            yield return Target.FromPos(end);
        }

    }
}
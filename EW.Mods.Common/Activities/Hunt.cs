using System;
using System.Collections.Generic;
using System.Linq;
using EW.Activities;
using EW.Mods.Common.Traits;
using EW.Traits;

namespace EW.Mods.Common.Activities
{
    public class Hunt:Activity
    {
        readonly IEnumerable<Actor> targets;
        readonly IMove move;

        public Hunt(Actor self)
        {
            move = self.Trait<IMove>();

            var attack = self.Trait<AttackBase>();
            targets = self.World.ActorsHavingTrait<Huntable>().Where(
                a => self != a &&
                !a.IsDead &&
                a.IsInWorld && 
                a.AppearsHostileTo(self) &&
                a.IsTargetableBy(self) && attack.HasAnyValidWeapons(Target.FromActor(a)));

        }

        public override Activity Tick(Actor self)
        {
            if (IsCanceled)
                return NextActivity;

            var target = targets.ClosestTo(self);

            if (target == null)
                return this;

            return ActivityUtils.SequenceActivities(
                new AttackMoveActivity(self, move.MoveTo(target.Location, 2)),
                new Wait(25), this);
        }

    }
}
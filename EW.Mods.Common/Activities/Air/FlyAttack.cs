using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Mods.Common.Traits;
using EW.Activities;
namespace EW.Mods.Common.Activities
{
    public class FlyAttack:Activity
    {

        readonly Target target;
        readonly Aircraft aircraft;
        readonly AttackPlane attackPlane;

        readonly bool autoReloads;
        int ticksUntilTurn;

        public FlyAttack(Actor self,Target target)
        {
            this.target = target;
            aircraft = self.Trait<Aircraft>();
            attackPlane = self.TraitOrDefault<AttackPlane>();
            ticksUntilTurn = attackPlane.AttackPlaneInfo.AttackTurnDelay;
            autoReloads = self.TraitsImplementing<AmmoPool>().All(p => p.AutoReloads);
        }

        public override Activity Tick(Actor self)
        {
            if (aircraft.ForceLanding)
            {
                Cancel(self);
                return NextActivity;

            }
            
            if (!target.IsValidFor(self))
                return NextActivity;


            if (!autoReloads && aircraft.Info.RearmBuildings.Any() &&
                attackPlane.Armaments.All(x => x.IsTraitPaused || !x.Weapon.IsValidAgainst(target, self.World, self)))
                return ActivityUtils.SequenceActivities(new ReturnToBase(self, aircraft.Info.AbortOnResupply), this);

            if (attackPlane != null)
                attackPlane.DoAttack(self, target);

            if(ChildActivity == null)
            {
                if (IsCanceled)
                    return NextActivity;

                if (attackPlane != null && target.IsInRange(self.CenterPosition, attackPlane.Armaments.Where(Exts.IsTraitEnabled).Select(a => a.Weapon.MinRange).Min()))
                    ChildActivity = ActivityUtils.SequenceActivities(new FlyTimed(ticksUntilTurn, self), new Fly(self, target), new FlyTimed(ticksUntilTurn, self));
                else
                    ChildActivity = ActivityUtils.SequenceActivities(new Fly(self, target), new FlyTimed(ticksUntilTurn, self));

                if (self.World.Map.DistanceAboveTerrain(self.CenterPosition).Length < aircraft.Info.MinAirborneAltitude)
                    ChildActivity = ActivityUtils.SequenceActivities(new TakeOff(self), ChildActivity);
            }

            ActivityUtils.RunActivity(self, ChildActivity);

            return this;
        }



    }
}
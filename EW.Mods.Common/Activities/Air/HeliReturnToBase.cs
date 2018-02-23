using System;
using EW.Activities;
using EW.Mods.Common.Traits;
using EW.Traits;
using System.Linq;

namespace EW.Mods.Common.Activities
{
    public class HeliReturnToBase:Activity
    {
        readonly Aircraft heli;
        readonly bool alwaysLand;
        readonly bool abortOnResupply;

        Actor dest;



        public HeliReturnToBase(Actor self,bool abortOnResupply,Actor dest = null,bool alwaysLand = true)
        {

            heli = self.Trait<Aircraft>();
            this.alwaysLand = alwaysLand;
            this.abortOnResupply = abortOnResupply;
            this.dest = dest;
        }


        public Actor ChooseHelipad(Actor self,bool unreservedOnly){

            var rearmBuildings = heli.Info.RearmBuildings;
            return self.World.Actors.Where(a => a.Owner == self.Owner
                                           && rearmBuildings.Contains(a.Info.Name)
                                           && (!unreservedOnly || !Reservable.IsReserved(self))).ClosestTo(self);
        }

        public override Activity Tick(Actor self)
        {
            if (heli.ForceLanding)
                return NextActivity;

            if (IsCanceled)
                return NextActivity;

            if(dest == null || dest.IsDead || Reservable.IsReserved(dest))
            {
                dest = ChooseHelipad(self, true);
            }

            var initialFacing = heli.Info.InitialFacing;

            if(dest == null || dest.IsDead)
            {
                var nearestHpad = ChooseHelipad(self, false);

                if (nearestHpad == null)
                    return ActivityUtils.SequenceActivities(new Turn(self, initialFacing), new HeliLand(self, true), NextActivity);
                else
                {
                    var distanceFromHelipad = (nearestHpad.CenterPosition - self.CenterPosition).HorizontalLength;
                    var distanceLength = heli.Info.WaitDistanceFromResupplyBase.Length;

                    if (distanceFromHelipad > distanceLength)
                    {
                        var randomPosition = WVec.FromPDF(self.World.SharedRandom, 2) * distanceLength / 1024;

                        var target = Target.FromPos(nearestHpad.CenterPosition + randomPosition);

                        return ActivityUtils.SequenceActivities(new HeliFly(self, target, WDist.Zero, heli.Info.WaitDistanceFromResupplyBase), this);
                    }
                    return this;
                }

            }

            var exit = dest.Info.FirstExitOrDefault(null);
            var offset = (exit != null) ? exit.SpawnOffset : WVec.Zero;

            if (ShouldLandAtBuilding(self, dest))
            {
                heli.MakeReservation(dest);

                return ActivityUtils.SequenceActivities(
                    new HeliFly(self, Target.FromPos(dest.CenterPosition + offset)),
                    new Turn(self, initialFacing),
                    new HeliLand(self, false));

            }


            return ActivityUtils.SequenceActivities(
                new HeliFly(self, Target.FromPos(dest.CenterPosition + offset)),
                NextActivity);

            
        }

        bool ShouldLandAtBuilding(Actor self,Actor dest)
        {
            if (alwaysLand)
                return true;

            if (heli.Info.RepairBuildings.Contains(dest.Info.Name) && self.GetDamageState() != DamageState.Undamaged)
                return true;

            return heli.Info.RearmBuildings.Contains(dest.Info.Name) && self.TraitsImplementing<AmmoPool>().Any(p => !p.AutoReloads && !p.FullAmmo());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using EW.Activities;
using EW.Mods.Common.Traits;
using EW.Traits;

namespace EW.Mods.Common.Activities
{
    public class ReturnToBase:Activity
    {
        readonly Aircraft plane;
        readonly AircraftInfo planeInfo;

        readonly bool alwaysLand;
        readonly bool abortOnResupply;
        bool isCalculated;
        Actor dest;
        WPos w1, w2, w3;

        public ReturnToBase(Actor self,bool abortOnResupply,Actor dest = null,bool alwaysLand = true)
        {
            this.dest = dest;
            this.alwaysLand = alwaysLand;
            this.abortOnResupply = abortOnResupply;
            plane = self.Trait<Aircraft>();
            planeInfo = self.Info.TraitInfo<AircraftInfo>();

        }


        public override Activity Tick(Actor self)
        {
            throw new NotImplementedException();
        }

        public static Actor ChooseAirfield(Actor self,bool unreservedOnly)
        {
            var rearmBuildings = self.Info.TraitInfo<AircraftInfo>().RearmBuildings;

            return self.World.ActorsHavingTrait<Reservable>()
                .Where(a => a.Owner == self.Owner 
                && rearmBuildings.Contains(a.Info.Name) && (!unreservedOnly || !Reservable.IsReserved(a))).ClosestTo(self);
        }
    }
}
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


        }
    }
}

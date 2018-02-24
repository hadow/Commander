using System;
using EW.Activities;
using EW.Mods.Common.Traits;
using EW.Traits;
namespace EW.Mods.Common.Activities
{
    public class ResupplyAircraft:CompositeActivity
    {

        public ResupplyAircraft(Actor self) { }


        protected override void OnFirstRun(Actor self)
        {
            var aircraft = self.Trait<Aircraft>();
            var host = aircraft.GetActorBelow();


            if (host == null)
                return;
        }




        public override Activity Tick(Actor self)
        {
            return NextActivity;
        }


    }
}
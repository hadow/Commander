using System;
using System.Linq;
using EW.Traits;
using EW.Mods.Common.Activities;
namespace EW.Mods.Common.Traits
{
    public class ReturnOnIdleInfo:ITraitInfo
    {

        public object Create(ActorInitializer init)
        {
            return new ReturnOnIdle(init.Self,this);
        }
    }

    public class ReturnOnIdle:INotifyIdle
    {

        readonly AircraftInfo aircraftInfo;

        public ReturnOnIdle(Actor self,ReturnOnIdleInfo info)
        {
            aircraftInfo = self.Info.TraitInfo<AircraftInfo>();

        }



        void INotifyIdle.TickIdle(Actor self)
        {

            if (self.World.Map.DistanceAboveTerrain(self.CenterPosition).Length < aircraftInfo.MinAirborneAltitude)
                return;

            var airfield = ReturnToBase.ChooseAirfield(self, true);

            if(airfield != null)
            {
                self.QueueActivity(new ReturnToBase(self, aircraftInfo.AbortOnResupply, airfield));
                self.QueueActivity(new ResupplyAircraft(self));
            }
            else
            {
                //nowhere to land,pick something friendly and circle over it.
                var someBuilding = self.World.ActorsHavingTrait<Building>().FirstOrDefault(a => a.Owner == self.Owner);

                if (someBuilding == null)
                    someBuilding = self.World.ActorsHavingTrait<Building>().FirstOrDefault(a => self.Owner.Stances[a.Owner] == Stance.Ally);

                if(someBuilding == null)
                {
                    self.QueueActivity(new FlyOffMap(self));
                    self.QueueActivity(new RemoveSelf());
                    return;
                }

                self.QueueActivity(new Fly(self, Target.FromActor(someBuilding)));
                self.QueueActivity(new FlyCircle(self));

            }

        }


    }
}
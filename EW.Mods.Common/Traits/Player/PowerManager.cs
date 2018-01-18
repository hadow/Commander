using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class PowerManagerInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new PowerManager(init.Self,this); }
    }
    public class PowerManager:INotifyCreated,ITick,ISync
    {


        public PowerManager(Actor self,PowerManagerInfo info){
            
        }

        void INotifyCreated.Created(Actor self){

            self.World.AddFrameEndTask(w=>UpdatePowerRequiringActors());
        }

        void ITick.Tick(Actor self){


        }


        void UpdatePowerRequiringActors(){
            
        }

        public void UpdateActor(Actor a){


        }
    }
}
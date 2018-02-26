using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class PowerManagerInfo : ITraitInfo,Requires<DeveloperModeInfo>
    {

        public readonly int AdviceInterval = 250;

        public readonly string SpeechNotification = "LowPower";


        public object Create(ActorInitializer init) { return new PowerManager(init.Self,this); }
    }
    public class PowerManager:INotifyCreated,ITick,ISync,IResolveOrder
    {
        readonly Actor self;
        readonly PowerManagerInfo info;
        readonly DeveloperMode devMode;

        readonly Dictionary<Actor, int> powerDrain = new Dictionary<Actor, int>();

        [Sync] int totalProvided;

        public int PowerProvided { get { return totalProvided; } }

        [Sync] int totalDrained;

        public int PowerDrained { get { return totalProvided; } }

        public PowerState PowerState
        {
            get
            {
                if (PowerProvided >= PowerDrained) return PowerState.Normal;

                if (PowerProvided > PowerDrained / 2) return PowerState.Low;

                return PowerState.Critical;
            }
        }


        int nextPowerAdviceTime = 0;
        bool isLowPower = false;
        bool wasLowPower = false;
        bool wasHackEnabled;

        public PowerManager(Actor self,PowerManagerInfo info)
        {
            this.self = self;
            this.info = info;

            devMode = self.Trait<DeveloperMode>();
            

            
        }

        void INotifyCreated.Created(Actor self){

            self.World.AddFrameEndTask(w=>UpdatePowerRequiringActors());
        }

        void ITick.Tick(Actor self)
        {


        }


        void UpdatePowerRequiringActors()
        {
            var traitPairs = self.World.ActorsWithTrait<INotifyPowerLevelChanged>()
                .Where(p => !p.Actor.IsDead && p.Actor.IsInWorld && p.Actor.Owner == self.Owner);

            foreach (var p in traitPairs)
                p.Trait.PowerLevelChanged(p.Actor);
        }

        public void UpdateActor(Actor a)
        {


        }

        void IResolveOrder.ResolveOrder(Actor self, NetWork.Order order)
        {
            if(order.OrderString == "PowerOutage")
            {

            }
        }
    }
}
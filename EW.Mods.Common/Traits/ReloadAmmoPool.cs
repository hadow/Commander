using System;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class ReloadAmmoPoolInfo:PausableConditionalTraitInfo{

        public readonly string AmmoPool = "primary";

        public readonly int Delay = 50;

        public readonly int Count = 1;

        public readonly bool ResetOnFire = false;

        public readonly string Sound = null;

        public override object Create(ActorInitializer init)
        {
            return new ReloadAmmoPool(this);
        }

        public override void RulesetLoaded(Ruleset rules, ActorInfo info)
        {


            base.RulesetLoaded(rules, info);
        }

    }


    public class ReloadAmmoPool:PausableConditionalTrait<ReloadAmmoPoolInfo>,ITick,INotifyCreated,INotifyAttack,ISync
    {

        AmmoPool ammoPool;

        [Sync]int remainingTicks;

        public ReloadAmmoPool(ReloadAmmoPoolInfo info):base(info){}


        void INotifyCreated.Created(Actor self){

            ammoPool = self.TraitsImplementing<AmmoPool>().Single(ap => ap.Info.Name == Info.AmmoPool);
            remainingTicks = Info.Delay;

        }

        void INotifyAttack.Attacking(Actor self,Target target,Armament a,Barrel barrel){

            if (Info.ResetOnFire)
                remainingTicks = Info.Delay;
            

        }

        void INotifyAttack.PreparingAttack(Actor self,Target target,Armament a,Barrel barrel){
            
        }

        void ITick.Tick(Actor self){

            if (IsTraitPaused || IsTraitDisabled)
                return;



        }



    }
}

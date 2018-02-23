using System;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class ReloadAmmoPoolInfo:PausableConditionalTraitInfo{

        public readonly string AmmoPool = "primary";

        public readonly int Delay = 50;

        [Desc("How much ammo is reloaded after Delay.")]
        public readonly int Count = 1;

        public readonly bool ResetOnFire = false;

        [Desc("Play this sound each time ammo is reloaded.")]
        public readonly string Sound = null;

        public override object Create(ActorInitializer init)
        {
            return new ReloadAmmoPool(this);
        }

        public override void RulesetLoaded(Ruleset rules, ActorInfo info)
        {
            if (info.TraitInfos<AmmoPoolInfo>().Count(ap => ap.Name == AmmoPool) != 1)
                throw new YamlException("ReloadsAmmoPool.AmmoPool requires exactly one AmmoPool with matching Name!");

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

        void INotifyAttack.PreparingAttack(Actor self,Target target,Armament a,Barrel barrel){}

        void ITick.Tick(Actor self){

            if (IsTraitPaused || IsTraitDisabled)
                return;

            Reload(self, Info.Delay, Info.Count, Info.Sound);
        }


        protected virtual void Reload(Actor self,int reloadDelay,int reloadCount,string sound)
        {
            if(!ammoPool.FullAmmo() && -- remainingTicks == 0)
            {
                remainingTicks = reloadDelay;

                if (!string.IsNullOrEmpty(sound))
                    WarGame.Sound.PlayToPlayer(SoundType.World, self.Owner, ammoPool.Info.RearmSound, self.CenterPosition);

                ammoPool.GiveAmmo(self, ammoPool.Info.ReloadCount);
            }
        }



    }
}

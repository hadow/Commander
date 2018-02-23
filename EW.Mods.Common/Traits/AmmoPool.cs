using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class AmmoPoolInfo : ITraitInfo
    {
        /// <summary>
        /// Name of this ammo pool,used to link armaments to this pool.
        /// </summary>
        public readonly string Name = "primary";

        public readonly string[] Armaments = { "primary", "secondary" };
        /// <summary>
        /// How much ammo does this pool contain when fully loaded.
        /// </summary>
        public readonly int Ammo = 1;

        /// <summary>
        /// Initial ammo the actor is created with.Defaults to Ammo.
        /// </summary>
        public readonly int InitialAmmo = -1;

        public readonly int PipCount = -1;

        public readonly PipType PipType = PipType.Green;

        public readonly PipType PipTypeEmpty = PipType.Transparent;


        /// <summary>
        /// How much ammo is reloaded after a certain period.
        /// </summary>
        public readonly int ReloadCount = 1;

        /// <summary>
        /// Sound to play for each reloaded ammo magazine.
        /// </summary>
        public readonly string RearmSound = null;

        public readonly int ReloadDelay = 50;

        /// <summary>
        /// Whether or not ammo is replenished on its own.
        /// </summary>
        public readonly bool SelfReloads = false;

        public readonly int SelfReloadDelay = 50;

        /// <summary>
        /// Whether or not reload timer should be reset when ammo has been fired.
        /// </summary>
        public readonly bool ResetOnFire = false;


        [GrantedConditionReference]
        public readonly string AmmoCondition = null;

        public object Create(ActorInitializer init) { return new AmmoPool(init.Self, this); }
    }
    public class AmmoPool : INotifyAttack, ITick, ISync,INotifyCreated
    {

        ConditionManager conditionManager;

        readonly Stack<int> tokens = new Stack<int>();

        public readonly AmmoPoolInfo Info;

        [Sync]
        int currentAmmo;

        [Sync]
        public int RemainingTicks;

        public int PreviousAmmo;

        public bool AutoReloads { get; private set; }


        public bool FullAmmo() { return currentAmmo == Info.Ammo; }

        public bool HasAmmo() { return currentAmmo > 0; }


        public AmmoPool(Actor self,AmmoPoolInfo info)
        {
            Info = info;

            if (Info.InitialAmmo < Info.Ammo && Info.InitialAmmo >= 0)
                currentAmmo = Info.InitialAmmo;
            else
                currentAmmo = Info.Ammo;


        }
        void ITick.Tick(Actor self)
        {
            if (!Info.SelfReloads)
                return;

            UpdateCondition(self);

        }

        void INotifyAttack.Attacking(Actor self,Target target,Armament a,Barrel barrel)
        {
            if (a != null && Info.Armaments.Contains(a.Info.Name))
                TakeAmmo(self, 1);
        }


        void INotifyAttack.PreparingAttack(Actor self, Target target, Armament a, Barrel barrel){}

        void INotifyCreated.Created(Actor self){

            conditionManager = self.TraitOrDefault<ConditionManager>();
            AutoReloads = self.TraitsImplementing<ReloadAmmoPool>().Any(r=>r.Info.AmmoPool == Info.Name && r.Info.RequiresCondition == null);

            UpdateCondition(self);

            RemainingTicks = Info.SelfReloadDelay;

        }


        void UpdateCondition(Actor self){

            if (conditionManager == null || string.IsNullOrEmpty(Info.AmmoCondition))
                return;

            while (currentAmmo > tokens.Count && tokens.Count < Info.Ammo)
                tokens.Push(conditionManager.GrantCondition(self, Info.AmmoCondition));


            while (currentAmmo < tokens.Count && tokens.Count > 0)
                conditionManager.RevokeCondition(self, tokens.Pop());


        }

        public bool TakeAmmo(Actor self,int count){

            if (currentAmmo <= 0 || count < 0)
                return false;

            currentAmmo = (currentAmmo - count).Clamp(0, Info.Ammo);
            UpdateCondition(self);
            return true;

        }

        public bool GiveAmmo(Actor self,int count){

            if (currentAmmo >= Info.Ammo || count < 0)
                return false;

            currentAmmo = (currentAmmo + count).Clamp(0, Info.Ammo);
            UpdateCondition(self);
            return true;
        }

    }
}
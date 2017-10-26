using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class AmmoPoolInfo : ITraitInfo
    {
        /// <summary>
        /// Name of this ammo pool,used to link armaments to this pool.
        /// </summary>
        public readonly string Name = "primary";

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

        public object Create(ActorInitializer init) { return new AmmoPool(init.Self, this); }
    }
    public class AmmoPool : INotifyAttack, ITick, ISync
    {
        public readonly AmmoPoolInfo Info;

        [Sync]
        public int CurrentAmmo;

        [Sync]
        public int RemainingTicks;

        public int PreviousAmmo;

        public AmmoPool(Actor self,AmmoPoolInfo info)
        {
            Info = info;

            if (Info.InitialAmmo < Info.Ammo && Info.InitialAmmo >= 0)
                CurrentAmmo = Info.InitialAmmo;
            else
                CurrentAmmo = Info.Ammo;

            RemainingTicks = Info.SelfReloadDelay;
            
        }
        void ITick.Tick(Actor self)
        {
            if (!Info.SelfReloads)
                return;

        }

        public void Attacking(Actor self,Target target,Armament a,Barrel barrel)
        {

        }

    }
}
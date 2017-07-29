using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class AmmoPoolInfo : ITraitInfo
    {
        public readonly string Name = "primary";

        public readonly int InitialAmmo = -1;

        public readonly int PipCount = -1;

        public readonly PipType PipType = PipType.Green;

        public readonly PipType PipTypeEmpty = PipType.Transparent;

        public readonly int ReloadCount = 1;

        public readonly string RearmSound = null;

        public readonly int ReloadDelay = 50;

        public readonly bool SelfReloads = false;

        public readonly int SelfReloadDelay = 50;

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

        }
        public void Tick(Actor self)
        {

        }

        public void Attacking(Actor self,Target target,Armament a,Barrel barrel)
        {

        }

    }
}
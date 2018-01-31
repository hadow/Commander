using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class PlayerResourcesInfo : ITraitInfo
    {

        [Translate]
        public readonly string DefaultCashDropdownLabel = "Starting Cash";

        public readonly string DefaultCashDropdownDescription = "Change the amount of cash that players start with";
        
        public readonly int[] SelectableCash = { 2500, 5000, 10000, 20000 };

        public readonly int DefaultCash = 5000;

        public readonly bool DefaultCashDropdownLocked = false;

        public readonly bool DefaultCashDropdownVisible = true;

        public readonly int DefaultCashDropdownDisplayOrder = 0;
        
        public object Create(ActorInitializer init) { return new PlayerResources(init.Self,this); }
    }




    public class  PlayerResources:ITick,ISync 
    {
        readonly PlayerResourcesInfo info;
        readonly Player owner;


        [Sync]
        public int Cash;
        [Sync]
        public int Resources;

        [Sync]
        public int ResourceCapacity;

        public int Earned;
        public int Spent;


        int lastNotificationTick;


        public PlayerResources(Actor self, PlayerResourcesInfo info)
        {

            this.info = info;
            owner = self.Owner;


        }

        public bool CanGiveResources(int amount)
        {
            return Resources + amount <= ResourceCapacity;
        }


        public void GiveResources(int num)
        {
            Resources += num;
            Earned += num;
            if (Resources > ResourceCapacity)
            {
                Earned -= Resources - ResourceCapacity;
                Resources = ResourceCapacity;
            }
        }

        public void GiveCash(int num)
        {

        }

        void ITick.Tick(Actor self)
        {
            ResourceCapacity = 0;

            foreach(var tp in self.World.ActorsWithTrait<IStoreResources>())
            {
                if (tp.Actor.Owner == owner)
                    ResourceCapacity += tp.Trait.Capacity;
            }

            if (Resources > ResourceCapacity)
                Resources = ResourceCapacity;
        }




    }
}
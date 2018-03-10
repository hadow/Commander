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

        [Desc("Speech notification to play when the player does not have any funds.")]
        public readonly string InsufficientFundsNotification = null;

        [Desc("Delay (in ticks) during which warnings will be muted.")]
        public readonly int InsufficientFundsNotificationDelay = 750;
        
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

            var startingCash = self.World.LobbyInfo.GlobalSettings
                .OptionOrDefault("startingcash", info.DefaultCash.ToString());

            if (!int.TryParse(startingCash, out Cash))
                Cash = info.DefaultCash;

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
            if (Cash < int.MaxValue)
            {
                try
                {
                    checked
                    {
                        Cash += num;
                    }
                }
                catch (OverflowException)
                {
                    Cash = int.MaxValue;
                }
            }

            if (Earned < int.MaxValue)
            {
                try
                {
                    checked
                    {
                        Earned += num;
                    }
                }
                catch (OverflowException)
                {
                    Earned = int.MaxValue;
                }
            }
        }

        public bool TakeResources(int num)
        {
            if (Resources < num) return false;
            Resources -= num;
            Spent += num;

            return true;
        }

        public bool TakeCash(int num,bool notifyLowFunds = false){

            if(Cash + Resources < num)
            {

                if (notifyLowFunds && !string.IsNullOrEmpty(info.InsufficientFundsNotification) &&
                    owner.World.WorldTick - lastNotificationTick >= info.InsufficientFundsNotificationDelay)
                {
                    lastNotificationTick = owner.World.WorldTick;
                    WarGame.Sound.PlayNotification(owner.World.Map.Rules, owner, "Speech", info.InsufficientFundsNotification, owner.Faction.InternalName);
                }
                return false;

            }
            // Spend ore before cash
            Resources -= num;
            Spent += num;

            if(Resources <= 0){

                Cash += Resources;
                Resources = 0;

            }

            return true;
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
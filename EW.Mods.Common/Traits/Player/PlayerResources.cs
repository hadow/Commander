using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class PlayerResourcesInfo : ITraitInfo
    {


        public readonly int[] SelectableCash = { 2500, 5000, 10000, 20000 };

        public readonly int DefaultCash = 5000;

        public readonly bool DefaultCashLocked = false;
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
        public int ResourcesCapacity;

        public int Earned;
        public int Spent;


        int lastNotificationTick;


        public PlayerResources(Actor self, PlayerResourcesInfo info)
        {

            this.info = info;
            owner = self.Owner;


        }

        void ITick.Tick(Actor self){}




    }
}
using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Adds capacity to a player's harvested resource limit.
    /// </summary>
    class StoresResourcesInfo : ITraitInfo
    {
        [FieldLoader.Require]
        public readonly int Capacity = 0;

        /// <summary>
        /// Number of little squares used to display how filled unit is.
        /// </summary>
        [FieldLoader.Require]
        public readonly int PipCount = 0;

        public readonly PipType PipColor = PipType.Yellow;

        public object Create(ActorInitializer init) { return new StoresResources(init.Self,this); }
    }
    class StoresResources:ISync,INotifyOwnerChanged,IStoreResources
    {
        readonly StoresResourcesInfo info;
        PlayerResources player;


        public int Stored { get { return player.ResourceCapacity == 0 ? 0 : (int)((long)info.Capacity * player.Resources / player.ResourceCapacity); } }

        public StoresResources(Actor self,StoresResourcesInfo info)
        {
            this.info = info;
            player = self.Owner.PlayerActor.Trait<PlayerResources>();
        }

        void INotifyOwnerChanged.OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
        {
            player = newOwner.PlayerActor.Trait<PlayerResources>();
        }

        int IStoreResources.Capacity { get { return info.Capacity; } }

    }
}
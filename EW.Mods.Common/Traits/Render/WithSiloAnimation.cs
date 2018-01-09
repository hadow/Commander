using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Render trait for buildings that change the sprite according to the remaining resource storage capacity across all depots.
    /// </summary>
    class WithSiloAnimationInfo : ITraitInfo,Requires<WithSpriteBodyInfo>,Requires<RenderSpritesInfo>
    {
        [SequenceReference]
        public readonly string Sequence = "stages";

        public readonly int Stages = 10;


        public object Create(ActorInitializer init) { return new WithSiloAnimation(init,this); }
    }
    class WithSiloAnimation:INotifyBuildComplete,INotifyOwnerChanged
    {
        readonly WithSiloAnimationInfo info;
        readonly WithSpriteBody wsb;

        PlayerResources playerResources;


        public WithSiloAnimation(ActorInitializer init,WithSiloAnimationInfo info)
        {
            this.info = info;
            wsb = init.Self.Trait<WithSpriteBody>();
            playerResources = init.Self.Owner.PlayerActor.Trait<PlayerResources>();
        }


        void INotifyBuildComplete.BuildingComplete(Actor self)
        {
            wsb.DefaultAnimation.PlayFetchIndex(wsb.NormalizeSequence(self, info.Sequence), () => playerResources.ResourceCapacity != 0
              ? ((info.Stages * wsb.DefaultAnimation.CurrentSequence.Length - 1) * playerResources.Resources) / (info.Stages * playerResources.ResourceCapacity) : 0);
        }

        void INotifyOwnerChanged.OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
        {
            playerResources = newOwner.PlayerActor.Trait<PlayerResources>();

            wsb.DefaultAnimation.PlayFetchIndex(wsb.NormalizeSequence(self, info.Sequence), () => playerResources.ResourceCapacity != 0
              ? ((info.Stages * wsb.DefaultAnimation.CurrentSequence.Length - 1) * playerResources.Resources) / (info.Stages * playerResources.ResourceCapacity) : 0);
        }
    }
}
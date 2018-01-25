using System;
using EW.Traits;
namespace EW.Mods.Common.Traits.Render
{


    public class WithHarvestAnimationInfo : ITraitInfo, Requires<WithSpriteBodyInfo>, Requires<HarvesterInfo>
    {
        [SequenceReference(null,true)]
        public readonly string[] PrefixByFullness = { "" };

        [SequenceReference]
        public readonly string HarvestSequence = "harvest";
        public object Create(ActorInitializer init) { return new WithHarvestAnimation(init, this); }
    }
    public class WithHarvestAnimation:ITick,INotifyHarvesterAction
    {
        public readonly WithHarvestAnimationInfo Info;
        readonly WithSpriteBody wsb;
        readonly Harvester harv;

        public WithHarvestAnimation(ActorInitializer init,WithHarvestAnimationInfo info)
        {
            Info = info;
            harv = init.Self.Trait<Harvester>();
            wsb = init.Self.Trait<WithSpriteBody>();
        }

        void ITick.Tick(Actor self)
        {

        }

        void INotifyHarvesterAction.Harvested(Actor self, ResourceType resource)
        {

        }

        void INotifyHarvesterAction.Docked()
        {

        }
        void INotifyHarvesterAction.Undocked()
        {

        }
    }
}
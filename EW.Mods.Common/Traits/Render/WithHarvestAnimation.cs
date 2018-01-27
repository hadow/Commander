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

        public bool IsModifying;
        public WithHarvestAnimation(ActorInitializer init,WithHarvestAnimationInfo info)
        {
            Info = info;
            harv = init.Self.Trait<Harvester>();
            wsb = init.Self.Trait<WithSpriteBody>();
        }


        protected virtual string NormalizeHarvesterSequence(Actor self,string baseSequence)
        {
            var desiredState = harv.Fullness * (Info.PrefixByFullness.Length - 1) / 100;
            var desiredPrefix = Info.PrefixByFullness[desiredState];

            if (wsb.DefaultAnimation.HasSequence(desiredPrefix + baseSequence))
                return desiredPrefix + baseSequence;
            else
                return baseSequence;
        }

        void ITick.Tick(Actor self)
        {
            var baseSequence = wsb.NormalizeSequence(self, wsb.Info.Sequence);
            var sequence = NormalizeHarvesterSequence(self, baseSequence);

            if (!IsModifying && wsb.DefaultAnimation.HasSequence(sequence) && wsb.DefaultAnimation.CurrentSequence.Name != sequence)
                wsb.DefaultAnimation.ReplaceAnim(sequence);
        }

        void INotifyHarvesterAction.Harvested(Actor self, ResourceType resource)
        {
            var baseSequence = wsb.NormalizeSequence(self, Info.HarvestSequence);
            var sequence = NormalizeHarvesterSequence(self, baseSequence);
            if(!IsModifying && wsb.DefaultAnimation.HasSequence(sequence))
            {
                IsModifying = true;
                wsb.PlayCustomAnimation(self, sequence, () => IsModifying = false);
            }
        }

        void INotifyHarvesterAction.Docked()
        {
            IsModifying = true;

        }
        void INotifyHarvesterAction.Undocked()
        {
            IsModifying = false;
        }
    }
}
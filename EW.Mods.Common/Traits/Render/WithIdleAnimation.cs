using System;
using EW.Traits;
namespace EW.Mods.Common.Traits.Render
{
    /// <summary>
    /// Periodically plays an idle animation,replacing the default body animation.
    /// 定期播放空闲动画，替换默认的正文动画。
    /// </summary>
    public class WithIdleAnimationInfo : ConditionalTraitInfo, Requires<WithSpriteBodyInfo>
    {
        [SequenceReference]
        public readonly string[] Sequences = { "active" };

        public readonly int Interval = 750;

        public override object Create(ActorInitializer init)
        {
            return new WithIdleAnimation(init.Self, this);
        }
    }
    public class WithIdleAnimation:ConditionalTrait<WithIdleAnimationInfo>,ITick,INotifyBuildComplete,INotifySold
    {

        readonly WithSpriteBody wsb;
        bool buildComplete;
        int ticks;

        public WithIdleAnimation(Actor self,WithIdleAnimationInfo info) : base(info)
        {
            wsb = self.Trait<WithSpriteBody>();
            buildComplete = !self.Info.HasTraitInfo<BuildingInfo>(); // always render instantly for units
            ticks = info.Interval;
        }

        void ITick.Tick(Actor self)
        {
            if (!buildComplete || IsTraitDisabled)
                return;

            if (--ticks <= 0)
            {
                wsb.PlayCustomAnimation(self, Info.Sequences.Random(WarGame.CosmeticRandom), () => wsb.CancelCustomAnimation(self));
                ticks = Info.Interval;
            }
        }


        void INotifyBuildComplete.BuildingComplete(Actor self)
        {
            buildComplete = true;
        }

        void INotifySold.Selling(Actor self)
        {
            buildComplete = false;
        }


        void INotifySold.Sold(Actor self) { }
    }
}
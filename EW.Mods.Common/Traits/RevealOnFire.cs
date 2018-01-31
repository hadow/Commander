using System;
using System.Linq;
using EW.Traits;
using EW.Mods.Common.Effects;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Reveal this actor to the target's owner when attacking.
    /// </summary>
    public class RevealOnFireInfo:ConditionalTraitInfo
    {

        /// <summary>
        /// The armament types which trigger revealing.
        /// </summary>
        public readonly string[] ArmamentNames = { "primary", "secondary" };

        /// <summary>
        /// Stances relative to the target player this actor will be revealed to during firing.
        /// </summary>
        public readonly Stance RevealForStancesRelativeToTarget = Stance.Ally;

        /// <summary>
        /// Duration of the reveal.
        /// </summary>
        public readonly int Duration = 25;

        /// <summary>
        /// Radius of the reveal around this actor.
        /// </summary>
        public readonly WDist Radius = new WDist(1536);


        public readonly bool RevealGeneratedShroud = true;
        public override object Create(ActorInitializer init)
        {
            return new RevealOnFire(init.Self, this);
        }

    }

    public class RevealOnFire : ConditionalTrait<RevealOnFireInfo>,INotifyAttack
    {
        readonly RevealOnFireInfo info;
        public RevealOnFire(Actor self,RevealOnFireInfo info) : base(info)
        {
            this.info = info;
        }


        void INotifyAttack.PreparingAttack(Actor self, Target target, Armament a, Barrel barrel) { }


        void INotifyAttack.Attacking(Actor self, Target target, Armament a, Barrel barrel)
        {
            if (IsTraitDisabled)
                return;

            if (!info.ArmamentNames.Contains(a.Info.Name))
                return;

            var targetPlayer = GetTargetPlayer(target);
            if(targetPlayer != null && targetPlayer.WinState == WinState.Undefined)
            {
                self.World.AddFrameEndTask(w => w.Add(new RevealShroudEffect(self.CenterPosition, info.Radius,
                    info.RevealGeneratedShroud ? Shroud.SourceType.Visibility : Shroud.SourceType.PassiveVisibility, targetPlayer,
                    info.RevealForStancesRelativeToTarget, duration: info.Duration)));
            }
        }

        Player GetTargetPlayer(Target target)
        {
            if (target.Type == TargetT.Actor)
                return target.Actor.Owner;

            return null;
        }
    }
}
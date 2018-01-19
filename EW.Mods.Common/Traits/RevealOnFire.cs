using System;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class RevealOnFireInfo:ConditionalTraitInfo
    {

        public readonly string[] ArmamentNames = { "primary", "secondary" };

        public readonly Stance RevealForStancesRelativeToTarget = Stance.Ally;

        public readonly int Duration = 25;

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
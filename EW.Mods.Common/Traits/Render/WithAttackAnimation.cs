using System;
using EW.Traits;
using System.Linq;
namespace EW.Mods.Common.Traits.Render
{
    public class WithAttackAnimationInfo:ConditionalTraitInfo,Requires<WithSpriteBodyInfo>,Requires<ArmamentInfo>,Requires<AttackBaseInfo>
    {

        public readonly string Armament = "primary";

        [SequenceReference]
        public readonly string AttackSequence = null;

        [SequenceReference]
        public readonly string AimSequence = null;

        [SequenceReference(null,true)]
        public readonly string ReloadPrefix = null;

        public readonly int Delay = 0;

        public readonly AttackDelayT DelayRelativeTo = AttackDelayT.Preparation;

        public readonly string Body = "body";


        public override object Create(ActorInitializer init)
        {
            return new WithAttackAnimation(init,this);
        }

        public override void RulesetLoaded(Ruleset rules, ActorInfo info)
        {

            var matches = info.TraitInfos<WithSpriteBodyInfo>().Count(w => w.Name == Body);

            if (matches != 1)
                throw new YamlException("WithAttackAnimation need exactly one sprite body with matching name.");

            base.RulesetLoaded(rules, info);
        }

    }

    public class WithAttackAnimation:ConditionalTrait<WithAttackAnimationInfo>,ITick,INotifyAttack
    {
        readonly AttackBase attack;
        readonly Armament armament;
        readonly WithSpriteBody wsb;
        readonly bool noAimOrReloadAnim;

        int tick;
        bool attackAnimPlaying;

        public WithAttackAnimation(ActorInitializer init,WithAttackAnimationInfo info) : base(info)
        {
            attack = init.Self.Trait<AttackBase>();
            armament = init.Self.TraitsImplementing<Armament>().Single(a => a.Info.Name == Info.Armament);
            wsb = init.Self.TraitsImplementing<WithSpriteBody>().First(w => w.Info.Name == Info.Body);

            noAimOrReloadAnim = string.IsNullOrEmpty(Info.AimSequence) && string.IsNullOrEmpty(Info.ReloadPrefix);
        }

        void INotifyAttack.Attacking(Actor self, Target target, Armament a, Barrel barrel)
        {
            if(Info.DelayRelativeTo == AttackDelayT.Attack)
            {
                if (Info.Delay > 0)
                    tick = Info.Delay;
                else
                    PlayAttackAnimation(self);
            }
        }

        void INotifyAttack.PreparingAttack(Actor self, Target target, Armament a, Barrel barrel)
        {
            if(Info.DelayRelativeTo == AttackDelayT.Preparation)
            {
                if (Info.Delay > 0)
                    tick = Info.Delay;
                else
                    PlayAttackAnimation(self);
            }
        }


        void PlayAttackAnimation(Actor self)
        {
            if(!IsTraitDisabled && !wsb.IsTraitDisabled && !string.IsNullOrEmpty(Info.AttackSequence))
            {
                attackAnimPlaying = true;
                wsb.PlayCustomAnimation(self, Info.AttackSequence, () => attackAnimPlaying = false);
            }
        }

        void ITick.Tick(Actor self)
        {
            if (Info.Delay > 0 && --tick == 0)
                PlayAttackAnimation(self);

            if (IsTraitDisabled || noAimOrReloadAnim || attackAnimPlaying || wsb.IsTraitDisabled)
                return;

            var sequence = wsb.Info.Sequence;

            if (!string.IsNullOrEmpty(Info.AimSequence) && attack.IsAniming)
                sequence = Info.AimSequence;

            var prefix = (armament.IsReloading && !string.IsNullOrEmpty(Info.ReloadPrefix)) ? Info.ReloadPrefix : "";

            if (!string.IsNullOrEmpty(prefix) && sequence != (prefix + sequence))
                sequence = prefix + sequence;

            wsb.DefaultAnimation.ReplaceAnim(sequence);

        }


    }
}
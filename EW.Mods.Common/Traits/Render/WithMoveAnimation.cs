using System;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Traits.Render
{

    public class WithMoveAnimationInfo : ConditionalTraitInfo, Requires<WithSpriteBodyInfo>, Requires<IMoveInfo>
    {
        [SequenceReference]
        public readonly string MoveSequence = "move";

        public readonly string Body = "body";


        public override object Create(ActorInitializer init) { return new WithMoveAnimation(init,this); }

        public override void RulesetLoaded(Ruleset rules, ActorInfo info)
        {

            var matches = info.TraitInfos<WithSpriteBodyInfo>().Count(w => w.Name == Body);
            if (matches != 1)
                throw new YamlException("WithMoveAnimation needs exactly one sprite body with matching name.");
            base.RulesetLoaded(rules, info);
        }
    }

    public class WithMoveAnimation:ConditionalTrait<WithMoveAnimationInfo>,ITick
    {
        readonly IMove movement;
        readonly WithSpriteBody wsb;


        public WithMoveAnimation(ActorInitializer init,WithMoveAnimationInfo info):base(info)
        {
            movement = init.Self.Trait<IMove>();
            wsb = init.Self.TraitsImplementing<WithSpriteBody>().First(w => w.Info.Name == Info.Body);

        }
        void ITick.Tick(Actor self)
        {
            if (IsTraitDisabled || wsb.IsTraitDisabled)
                return;

            var isMoving = movement.IsMoving && !self.IsDead;

            if (isMoving ^ (wsb.DefaultAnimation.CurrentSequence.Name != Info.MoveSequence))
                return;
            wsb.DefaultAnimation.ReplaceAnim(isMoving ? Info.MoveSequence : wsb.Info.Sequence);
        }

    }
}
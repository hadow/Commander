using System;
using EW.Traits;
using EW.Support;
namespace EW.Mods.Common.Traits
{
    public abstract class PausableConditionalTraitInfo:ConditionalTraitInfo
    {
        public bool PausedByDefault { get; private set; }

        [ConsumedConditionReference]
        public readonly BooleanExpression PauseCondition = null;



        public override void RulesetLoaded(Ruleset rules, ActorInfo info)
        {
            base.RulesetLoaded(rules, info);
            PausedByDefault = PauseCondition != null && PauseCondition.Evaluate(VariableExpression.NoVariables);

        }
    }


    public abstract class PausableConditionalTrait<InfoType> : ConditionalTrait<InfoType> where InfoType : PausableConditionalTraitInfo
    {
        [Sync]
        public bool IsTraitPaused { get; private set; }



        protected PausableConditionalTrait(InfoType info) : base(info)
        {
            IsTraitPaused = info.PausedByDefault;

        }


        protected override void Created(Actor self)
        {
            base.Created(self);
            if (Info.PauseCondition == null)
                TraitResumed(self);


                
        }


        protected virtual void TraitResumed(Actor self){}

        protected virtual void TraitPaused(Actor self){}

    }
}
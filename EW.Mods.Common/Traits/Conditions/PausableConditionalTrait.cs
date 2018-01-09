using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Support;
namespace EW.Mods.Common.Traits
{
    public abstract class PausableConditionalTraitInfo:ConditionalTraitInfo
    {
        public bool PausedByDefault { get; private set; }

        /// <summary>
        /// Boolean expression defining the condition to pause this trait.
        /// </summary>
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


        public override IEnumerable<VariableObserver> GetVariableObservers()
        {
            foreach (var observer in base.GetVariableObservers())
                yield return observer;

            if (Info.PauseCondition != null)
                yield return new VariableObserver(PauseConditionsChanged, Info.PauseCondition.Variables);
        }


        void PauseConditionsChanged(Actor self,IReadOnlyDictionary<string,int> conditions)
        {
            var wasPaused = IsTraitPaused;
            IsTraitPaused = Info.PauseCondition.Evaluate(conditions);

            if(IsTraitPaused != wasPaused)
            {
                if (wasPaused)
                    TraitResumed(self);
                else
                    TraitPaused(self);
            }
        }


        protected virtual void TraitResumed(Actor self){}

        protected virtual void TraitPaused(Actor self){}

    }
}
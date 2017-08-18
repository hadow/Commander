using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Support;
namespace EW.Mods.Common.Traits
{

    public abstract class ConditionalTraitInfo:IObservesVariablesInfo,IRulesetLoaded
    {
        protected static readonly IReadOnlyDictionary<string, int> NoConditions = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>());

        [ConsumedConditionReference]
        /// <summary>
        /// Boolean expression defining the condition to enable this trait.
        /// </summary>
        public readonly BooleanExpression RequiresCondition = null;

        public bool EnabledByDefault { get; private set; }

        public virtual void RulesetLoaded(Ruleset rules,ActorInfo ai)
        {

        }
        public abstract object Create(ActorInitializer init);
    }

    /// <summary>
    /// Abstract base for enabling and disabling trait using conditions.
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ConditionalTrait<T>:ISync where T:ConditionalTraitInfo
    {
        public readonly T Info;

        [Sync]
        public bool IsTraitDisabled { get; private set; }

        public ConditionalTrait(T info)
        {
            Info = info;

            IsTraitDisabled = info.RequiresCondition != null;
        }

        public virtual IEnumerable<VariableObserver> GetVariableObservers()
        {
            if (Info.RequiresCondition != null)
                yield return new VariableObserver(RequiredConditionsChanged, Info.RequiresCondition.Variables);
        }


        void RequiredConditionsChanged(Actor self,IReadOnlyDictionary<string,int> conditions)
        {
            if (Info.RequiresCondition == null)
                return;

            var wasDisabled = IsTraitDisabled;
            IsTraitDisabled = !Info.RequiresCondition.Evaluate(conditions);

            if(IsTraitDisabled != wasDisabled)
            {
                if (wasDisabled)
                    TraitEnabled(self);
                else
                    TraitDisabled(self);
            }
        }



        protected virtual void TraitEnabled(Actor self) { }

        protected virtual void TraitDisabled(Actor self) { }

    }
}

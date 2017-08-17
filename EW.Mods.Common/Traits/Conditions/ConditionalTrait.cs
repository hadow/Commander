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

    public abstract class ConditionalTrait<T>:ISync where T:ConditionalTraitInfo
    {
        public ConditionalTrait(T info)
        {
        }
    }
}

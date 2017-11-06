using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    /// <summary>
    /// Grants a condition while the trait is active.
    /// </summary>
    class GrantConditionInfo:ConditionalTraitInfo
    {
        
        [GrantedConditionReference]
        [FieldLoader.Require]
        public readonly string Condition = null;//Condition to grant.
        public override object Create(ActorInitializer init)
        {
            return new GrantCondition(this);
        }

    }


    class GrantCondition : ConditionalTrait<GrantConditionInfo>
    {
        ConditionManager conditionManager;

        int conditionToken = ConditionManager.InvalidConditionToken;

        public GrantCondition(GrantConditionInfo info) : base(info) { }



        protected override void Created(Actor self)
        {
            conditionManager = self.Trait<ConditionManager>();
            base.Created(self);
        }


        protected override void TraitEnabled(Actor self)
        {
            if (conditionToken == ConditionManager.InvalidConditionToken)
                conditionToken = conditionManager.GrantCondition(self, Info.Condition);
        }

        protected override void TraitDisabled(Actor self)
        {
            if (conditionToken == ConditionManager.InvalidConditionToken)
                return;

            conditionToken = conditionManager.RevokeCondition(self, conditionToken);
        }
    }
}
using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class GrantConditionOnPowerStateInfo:ConditionalTraitInfo
    {

        [FieldLoader.Require]
        [GrantedConditionReference]
        public readonly string Condition = null;


        [FieldLoader.Require]
        public readonly PowerState ValidPowerStates = PowerState.Low | PowerState.Critical;
        public override object Create(ActorInitializer init)
        {
            return new GrantConditionOnPowerState(init.Self,this);
        }
    }


    public class GrantConditionOnPowerState:ConditionalTrait<GrantConditionOnPowerStateInfo>,INotifyOwnerChanged,INotifyPowerLevelChanged
    {
        PowerManager playerPower;
        ConditionManager conditionManager;
        int conditionToken = ConditionManager.InvalidConditionToken;

        bool validPowerState;


        public GrantConditionOnPowerState(Actor self,GrantConditionOnPowerStateInfo info) : base(info)
        {
            playerPower = self.Owner.PlayerActor.Trait<PowerManager>();

        }


        protected override void Created(Actor self)
        {
            base.Created(self);

            conditionManager = self.TraitOrDefault<ConditionManager>();
            Update(self);
        }

        protected override void TraitDisabled(Actor self)
        {
            Update(self);
        }

        protected override void TraitEnabled(Actor self)
        {
            Update(self);
        }


        void INotifyPowerLevelChanged.PowerLevelChanged(Actor self)
        {
            Update(self);
        }

        void INotifyOwnerChanged.OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
        {
            playerPower = newOwner.PlayerActor.Trait<PowerManager>();
            Update(self);
        }
        void Update(Actor self)
        {
            if (conditionManager == null)
                return;

            validPowerState = !IsTraitDisabled && Info.ValidPowerStates.HasFlag(playerPower.PowerState);

            if (validPowerState && conditionToken == ConditionManager.InvalidConditionToken)
                conditionToken = conditionManager.GrantCondition(self, Info.Condition);
            else if (!validPowerState && conditionToken != ConditionManager.InvalidConditionToken)
                conditionToken = conditionManager.RevokeCondition(self, conditionToken);
        }


    }
}
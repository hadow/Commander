using System;
using System.Drawing;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    
    public class AffectedByPowerOutageInfo:ConditionalTraitInfo
    {

        [GrantedConditionReference]
        [Desc("The condition to grant while there is a power outage.")]
        public readonly string Condition = null;


        public override object Create(ActorInitializer init)
        {
            return new AffectedByPowerOutage(init.Self, this);
        }
    }

    public class AffectedByPowerOutage : ConditionalTrait<AffectedByPowerOutageInfo>,INotifyOwnerChanged,INotifyCreated,INotifyAddedToWorld,ISelectionBar
    {

        PowerManager playerPower;
        ConditionManager conditionManager;
        int token = ConditionManager.InvalidConditionToken;

        public AffectedByPowerOutage(Actor self,AffectedByPowerOutageInfo info) : base(info)
        {
            playerPower = self.Owner.PlayerActor.Trait<PowerManager>();

        }


        void INotifyAddedToWorld.AddedToWorld(Actor self) { UpdateStatus(self); }
        protected override void TraitEnabled(Actor self) { UpdateStatus(self); }
        protected override void TraitDisabled(Actor self) { Revoke(self); }

        protected override void Created(Actor self)
        {
            conditionManager = self.TraitOrDefault<ConditionManager>();

            base.Created(self);
        }


        void INotifyOwnerChanged.OnOwnerChanged(Actor self,Player oldOwner,Player newOwner){


            playerPower = newOwner.PlayerActor.Trait<PowerManager>();

        }


        public void UpdateStatus(Actor self){

            if (!IsTraitDisabled && playerPower.PowerOutageRemainingTicks > 0)
                Grant(self);
            else
                Revoke(self);
        }

        void Grant(Actor self)
        {
            if (token == ConditionManager.InvalidConditionToken)
                token = conditionManager.GrantCondition(self, Info.Condition);
        }

        void Revoke(Actor self)
        {
            if (token != ConditionManager.InvalidConditionToken)
                token = conditionManager.RevokeCondition(self, token);
        }


        float ISelectionBar.GetValue()
        {
            if (IsTraitDisabled || playerPower.PowerOutageRemainingTicks <= 0)
                return 0;

            return (float)playerPower.PowerOutageRemainingTicks / playerPower.PowerOutageTotalTicks;
        }

        Color ISelectionBar.GetColor()
        {
            return Color.Yellow;
        }

        bool ISelectionBar.DisplayWhenEmpty { get { return false; } }

    }
}
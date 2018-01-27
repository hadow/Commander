using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Actor must charge up its armaments before firing.
    /// 蓄力发射
    /// </summary>
    public class AttackChargesInfo : AttackOmniInfo
    {
        /// <summary>
        /// Amount of charge required to attack.
        /// </summary>
        public readonly int ChargeLevel = 25;
        /// <summary>
        /// Amount to increase the charge level each tick with a valid target.
        /// </summary>
        public readonly int ChargeRate = 1;

        public readonly int DischargeRate = 1;

        [GrantedConditionReference]
        public readonly string ChargingCondition = null;



        public override object Create(ActorInitializer init)
        {
            return new AttackCharges(init.Self, this);
        }
    }
    public class AttackCharges:AttackOmni,INotifyCreated,ITick,INotifyAttack
    {
        readonly AttackChargesInfo info;
        ConditionManager conditionManager;
        int chargingToken = ConditionManager.InvalidConditionToken;
        bool charging;

        public int ChargeLevel { get; private set; }
        public AttackCharges(Actor self,AttackChargesInfo info) : base(self, info)
        {
            this.info = info;
        }


        protected override void Created(Actor self)
        {
            conditionManager = self.TraitOrDefault<ConditionManager>();

            base.Created(self);
        }

        void ITick.Tick(Actor self)
        {
            //Stop charging when we lose our target
            charging &= self.CurrentActivity is SetTarget;
            var delta = charging ? info.ChargeRate : -info.DischargeRate;
            ChargeLevel = (ChargeLevel + delta).Clamp(0, info.ChargeLevel);

            if (ChargeLevel > 0 && conditionManager != null && !string.IsNullOrEmpty(info.ChargingCondition) && chargingToken == ConditionManager.InvalidConditionToken)
                chargingToken = conditionManager.GrantCondition(self, info.ChargingCondition);

            if (ChargeLevel == 0 && conditionManager != null && chargingToken != ConditionManager.InvalidConditionToken)
                chargingToken = conditionManager.RevokeCondition(self, chargingToken);
        }


        protected override bool CanAttack(Actor self, Target target)
        {
            charging = base.CanAttack(self, target) && IsReachableTarget(target, true);
            return ChargeLevel >= info.ChargeLevel && charging;
        }


        void INotifyAttack.Attacking(Actor self, Target target, Armament a, Barrel barrel) { ChargeLevel = 0; }

        void INotifyAttack.PreparingAttack(Actor self, Target target, Armament a, Barrel barrel) { }
    }
}
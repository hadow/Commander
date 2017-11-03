using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class GrantConditionOnDamageStateInfo : ITraitInfo, Requires<HealthInfo>
    {

        [FieldLoader.Require]
        [GrantedConditionReference]
        public readonly string Condition = null;


        /// <summary>
        /// Play a random sound from this list when enabled.
        /// </summary>
        public readonly string[] EnabledSounds = { };


        /// <summary>
        /// Play a random sound from this list when disabled.
        /// </summary>
        public readonly string[] DisabledSounds = { };


        /// <summary>
        /// Levels of damage at which to grant the condition.
        /// </summary>
        public readonly DamageState ValidDamageStates = DamageState.Heavy | DamageState.Critical;


        public readonly bool GrantPermanently = false;
        public object Create(ActorInitializer init) { return new GrantConditionOnDamageState(init.Self,this); }
    }
    public class GrantConditionOnDamageState:INotifyCreated
    {

        public GrantConditionOnDamageState(Actor self,GrantConditionOnDamageStateInfo info)
        {
            
        }

        void INotifyCreated.Created(Actor self)
        {

        }
    }
}
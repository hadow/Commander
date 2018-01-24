using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Scripting;
using EW.Mods.Common.Traits;
using EW.Mods.Common.Activities;

namespace EW.Mods.Common.Scripting
{
    [ScriptPropertyGroup("Combat")]
    public class CombatProperties:ScriptActorProperties,Requires<AttackBaseInfo>,Requires<IMoveInfo>
    {

        readonly IMove move;

        public CombatProperties(ScriptContext context,Actor self) : base(context, self)
        {
            move = self.Trait<IMove>();
        }

        /// <summary>
        /// Seek out and attack nearby targets.
        /// </summary>
        /// 
        [ScriptActorPropertyActivity]
        public void Hunt()
        {
            Self.QueueActivity(new Hunt(Self));
        }
    }
}